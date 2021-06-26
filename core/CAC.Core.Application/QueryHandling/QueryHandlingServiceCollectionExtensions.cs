using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CAC.Core.Application;
using CAC.Core.Application.QueryHandling;
using CAC.Core.Application.QueryHandling.Behaviors;
using Microsoft.Extensions.DependencyInjection.Extensions;

// reflection is safe here
#pragma warning disable S3011

// ReSharper disable once CheckNamespace (it's a convention to place service collection extensions in this namespace)
namespace Microsoft.Extensions.DependencyInjection
{
    public static class QueryHandlingServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryHandler<TQueryHandler>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
            where TQueryHandler : class, IQueryHandler
        {
            var queryHandlerInterfaces = typeof(TQueryHandler).GetInterfaces().Where(IsQueryHandlerInterface).ToList();

            if (queryHandlerInterfaces.Count < 1)
            {
                throw new InvalidOperationException($"type {typeof(TQueryHandler).Name} does not implement a generic query handler interface");
            }

            if (queryHandlerInterfaces.Count > 1)
            {
                throw new InvalidOperationException($"type {typeof(TQueryHandler).Name} implements more than one query handler interface");
            }

            var queryHandlerInterface = queryHandlerInterfaces.Single();

            return services.RegisterQueryHandler(queryHandlerInterface, typeof(TQueryHandler), serviceLifetime ?? ServiceLifetime.Transient);

            static bool IsQueryHandlerInterface(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
        }

        public static IServiceCollection AddQueryHandlerBehavior(this IServiceCollection services, Type behaviorType, ServiceLifetime? serviceLifetime = null)
        {
            var behaviorInterfaces = behaviorType.GetInterfaces().Where(IsBehavior).ToList();

            if (behaviorInterfaces.Count < 1)
            {
                throw new InvalidOperationException($"type {behaviorType.Name} does not implement the query handler behavior interface");
            }

            if (behaviorInterfaces.Count > 1)
            {
                throw new InvalidOperationException($"type {behaviorType.Name} implements more than one query handler behavior interface");
            }

            var behaviorInterface = behaviorInterfaces.Single();
            var attributeType = behaviorInterface.GetGenericArguments()[2];

            var descriptor = new QueryHandlerBehaviorServiceDescriptor(behaviorType, behaviorType, serviceLifetime ?? ServiceLifetime.Transient, attributeType);
            return services.Replace(descriptor);

            static bool IsBehavior(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandlerBehavior<,,>);
        }

        private static IServiceCollection RegisterQueryHandler(this IServiceCollection services, Type queryHandlerInterface, Type implementationType, ServiceLifetime serviceLifetime)
        {
            var queryType = queryHandlerInterface.GetGenericArguments().First();
            var responseType = queryHandlerInterface.GetGenericArguments().Skip(1).First();

            var method = typeof(QueryHandlingServiceCollectionExtensions).GetMethod(nameof(RegisterQueryHandlerGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            var typedMethod = method?.MakeGenericMethod(queryType, responseType);
            var result = typedMethod?.Invoke(null, new object[] { services, queryHandlerInterface, implementationType, serviceLifetime });
            return result as IServiceCollection ?? services;
        }

        private static IServiceCollection RegisterQueryHandlerGeneric<TQuery, TResponse>(this IServiceCollection services,
                                                                                         Type queryHandlerInterface,
                                                                                         Type implementationType,
                                                                                         ServiceLifetime serviceLifetime)
            where TQuery : notnull
        {
            var descriptor = new QueryHandlerServiceDescriptor(queryHandlerInterface, implementationType, serviceLifetime, implementationType);

            var relevantBehaviors = GetRelevantBehaviors<TQuery, TResponse>(services, implementationType);

            if (!relevantBehaviors.Any())
            {
                return services.Replace(descriptor);
            }

            var decoratorType = typeof(QueryHandlerBehaviorInvocationDecorator<TQuery, TResponse>);
            var decoratorDescriptor = new QueryHandlerServiceDescriptor(queryHandlerInterface, InstantiateWithBehaviors, serviceLifetime, implementationType);

            return services.Replace(decoratorDescriptor);

            object InstantiateWithBehaviors(IServiceProvider p)
            {
                var adaptedInstance = p.GetInstance(descriptor);
                var behaviors = relevantBehaviors.Select(b => b(p)).ToList();

                var instance = Activator.CreateInstance(decoratorType, adaptedInstance, behaviors);

                if (instance == null)
                {
                    throw new InvalidOperationException($"failed to instantiate query handler behavior invocation decorator for type {queryHandlerInterface.Name}!");
                }

                return instance;
            }
        }

        private static IReadOnlyCollection<Func<IServiceProvider, QueryHandlerBehaviorInvocation<TQuery, TResponse>>> GetRelevantBehaviors<TQuery, TResponse>(
            this IServiceCollection services, Type implementationType)
            where TQuery : notnull
        {
            var markerAttributes = GetExecuteQueryMethodInfo()?.GetCustomAttributes<QueryHandlerBehaviorAttribute>() ?? new List<QueryHandlerBehaviorAttribute>();
            var descriptorsByAttributeType = services.OfType<QueryHandlerBehaviorServiceDescriptor>().ToDictionary(d => d.MarkerAttributeType);

            var result = new List<Func<IServiceProvider, QueryHandlerBehaviorInvocation<TQuery, TResponse>>>();

            foreach (var attribute in markerAttributes)
            {
                if (!descriptorsByAttributeType.ContainsKey(attribute.GetType()))
                {
                    throw new InvalidOperationException($"behavior for attribute type {attribute.GetType().Name} is not registered!");
                }

                var descriptor = descriptorsByAttributeType[attribute.GetType()];

                var method = typeof(QueryHandlingServiceCollectionExtensions).GetMethod(nameof(CreateBehaviorInvocationFactory), BindingFlags.Static | BindingFlags.NonPublic);
                var typedMethod = method?.MakeGenericMethod(typeof(TQuery), typeof(TResponse), attribute.GetType());
                var invocationResult = typedMethod?.Invoke(null, new object[] { descriptor, attribute });

                result.Add((Func<IServiceProvider, QueryHandlerBehaviorInvocation<TQuery, TResponse>>)invocationResult!);
            }

            return result;

            MethodInfo? GetExecuteQueryMethodInfo() => implementationType.GetMethod(nameof(IQueryHandler<string, string>.ExecuteQuery), BindingFlags.Instance | BindingFlags.Public);
        }

        private static Func<IServiceProvider, QueryHandlerBehaviorInvocation<TQuery, TResponse>> CreateBehaviorInvocationFactory<TQuery, TResponse, TAttribute>(
            ServiceDescriptor descriptor, TAttribute attribute)
            where TQuery : notnull
            where TAttribute : Attribute
        {
            var genericBehaviorType = descriptor.ImplementationType!;
            var behaviorType = genericBehaviorType.MakeGenericType(typeof(TQuery), typeof(TResponse));
            var concreteDescriptor = ServiceDescriptor.Describe(behaviorType, behaviorType, descriptor.Lifetime);

            return p =>
            {
                var behaviorInstance = (IQueryHandlerBehavior<TQuery, TResponse, TAttribute>)p.GetInstance(concreteDescriptor);
                return (c, n, t) => behaviorInstance.InterceptQuery(c, n, attribute, t);
            };
        }

        private sealed class QueryHandlerServiceDescriptor : ServiceDescriptor
        {
            public QueryHandlerServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, Type originalHandlerType)
                : base(serviceType, implementationType, lifetime)
            {
                OriginalHandlerType = originalHandlerType;
            }

            public QueryHandlerServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, Type originalHandlerType)
                : base(serviceType, factory, lifetime)
            {
                OriginalHandlerType = originalHandlerType;
            }

            public Type OriginalHandlerType { get; }
        }

        private sealed class QueryHandlerBehaviorServiceDescriptor : ServiceDescriptor
        {
            public QueryHandlerBehaviorServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, Type markerAttributeType)
                : base(serviceType, implementationType, lifetime)
            {
                MarkerAttributeType = markerAttributeType;
            }

            public Type MarkerAttributeType { get; }
        }
    }
}
