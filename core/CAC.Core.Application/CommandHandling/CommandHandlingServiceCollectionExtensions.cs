using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Application.CommandHandling.Behaviors;
using Microsoft.Extensions.DependencyInjection.Extensions;

// reflection is safe here
#pragma warning disable S3011

// ReSharper disable once CheckNamespace (it's a convention to place service collection extensions in this namespace)
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CommandHandlingServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandHandler<TCommandHandler>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
            where TCommandHandler : class, ICommandHandler
        {
            var commandHandlerInterfaces = typeof(TCommandHandler).GetInterfaces().Where(IsCommandHandlerInterface).ToList();

            if (commandHandlerInterfaces.Count < 1)
            {
                throw new InvalidOperationException($"type {typeof(TCommandHandler).Name} does not implement a generic command handler interface");
            }

            if (commandHandlerInterfaces.Count > 1)
            {
                throw new InvalidOperationException($"type {typeof(TCommandHandler).Name} implements more than one command handler interface");
            }

            var commandHandlerInterface = commandHandlerInterfaces.Single();

            return services.RegisterCommandHandler(commandHandlerInterface, typeof(TCommandHandler), serviceLifetime ?? ServiceLifetime.Transient);

            static bool IsCommandHandlerInterface(Type i) =>
                i.IsGenericType
                && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
        }

        public static IServiceCollection AddCommandHandlerBehavior(this IServiceCollection services, Type behaviorType, ServiceLifetime? serviceLifetime = null)
        {
            var behaviorInterfaces = behaviorType.GetInterfaces().Where(IsBehavior).ToList();

            if (behaviorInterfaces.Count < 1)
            {
                throw new InvalidOperationException($"type {behaviorType.Name} does not implement the command handler behavior interface");
            }

            if (behaviorInterfaces.Count > 1)
            {
                throw new InvalidOperationException($"type {behaviorType.Name} implements more than one command handler behavior interface");
            }

            var behaviorInterface = behaviorInterfaces.Single();
            var attributeType = behaviorInterface.GetGenericArguments()[2];

            var descriptor = new CommandHandlerBehaviorServiceDescriptor(behaviorType, behaviorType, serviceLifetime ?? ServiceLifetime.Transient, attributeType);
            return services.Replace(descriptor);

            static bool IsBehavior(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandlerBehavior<,,>);
        }

        private static IServiceCollection RegisterCommandHandler(this IServiceCollection services, Type commandHandlerInterface, Type implementationType, ServiceLifetime serviceLifetime)
        {
            var commandType = commandHandlerInterface.GetGenericArguments().First();
            var responseType = commandHandlerInterface.GetGenericArguments().Skip(1).FirstOrDefault() ?? typeof(UnitCommandResponse);

            var method = typeof(CommandHandlingServiceCollectionExtensions).GetMethod(nameof(RegisterCommandHandlerGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            var typedMethod = method?.MakeGenericMethod(commandType, responseType);
            var result = typedMethod?.Invoke(null, new object[] { services, commandHandlerInterface, implementationType, serviceLifetime });
            return result as IServiceCollection ?? services;
        }

        private static IServiceCollection RegisterCommandHandlerGeneric<TCommand, TResponse>(this IServiceCollection services,
                                                                                             Type commandHandlerInterface,
                                                                                             Type implementationType,
                                                                                             ServiceLifetime serviceLifetime)
            where TCommand : notnull
        {
            var originalDescriptor = new CommandHandlerServiceDescriptor(commandHandlerInterface, implementationType, serviceLifetime, implementationType);

            var relevantBehaviors = GetRelevantBehaviors<TCommand, TResponse>(services, implementationType);

            if (!relevantBehaviors.Any())
            {
                return services.Replace(originalDescriptor);
            }

            var adaptedDescriptor = originalDescriptor;

            if (typeof(TResponse) == typeof(UnitCommandResponse))
            {
                var interfaceType = typeof(ICommandHandler<TCommand, TResponse>);
                var adapterType = typeof(CommandHandlerWithoutResponseAdapter<TCommand>);
                adaptedDescriptor = new(interfaceType, p => p.CreateInstance(adapterType, p.GetInstance(originalDescriptor)), serviceLifetime, implementationType);
            }

            var decoratorType = typeof(CommandHandlerBehaviorInvocationDecorator<TCommand, TResponse>);
            var decoratorDescriptor = new CommandHandlerServiceDescriptor(commandHandlerInterface, InstantiateWithBehaviors, serviceLifetime, implementationType);

            return services.Replace(decoratorDescriptor);

            object InstantiateWithBehaviors(IServiceProvider p)
            {
                var adaptedInstance = p.GetInstance(adaptedDescriptor);
                var behaviors = relevantBehaviors.Select(b => b(p)).ToList();

                var instance = Activator.CreateInstance(decoratorType, adaptedInstance, behaviors);

                if (instance == null)
                {
                    throw new InvalidOperationException($"failed to instantiate command handler behavior invocation decorator for type {commandHandlerInterface.Name}!");
                }

                return instance;
            }
        }

        private static IReadOnlyCollection<Func<IServiceProvider, CommandHandlerBehaviorInvocation<TCommand, TResponse>>> GetRelevantBehaviors<TCommand, TResponse>(
            this IServiceCollection services, Type implementationType)
            where TCommand : notnull
        {
            var markerAttributes = GetExecuteCommandMethodInfo()?.GetCustomAttributes<CommandHandlerBehaviorAttribute>() ?? new List<CommandHandlerBehaviorAttribute>();
            var descriptorsByAttributeType = services.OfType<CommandHandlerBehaviorServiceDescriptor>().ToDictionary(d => d.MarkerAttributeType);

            var result = new List<Func<IServiceProvider, CommandHandlerBehaviorInvocation<TCommand, TResponse>>>();

            foreach (var attribute in markerAttributes)
            {
                if (!descriptorsByAttributeType.ContainsKey(attribute.GetType()))
                {
                    throw new InvalidOperationException($"behavior for attribute type {attribute.GetType().Name} is not registered!");
                }

                var descriptor = descriptorsByAttributeType[attribute.GetType()];

                var method = typeof(CommandHandlingServiceCollectionExtensions).GetMethod(nameof(CreateBehaviorInvocationFactory), BindingFlags.Static | BindingFlags.NonPublic);
                var typedMethod = method?.MakeGenericMethod(typeof(TCommand), typeof(TResponse), attribute.GetType());
                var invocationResult = typedMethod?.Invoke(null, new object[] { descriptor, attribute });

                result.Add((Func<IServiceProvider, CommandHandlerBehaviorInvocation<TCommand, TResponse>>)invocationResult!);
            }

            return result;

            MethodInfo? GetExecuteCommandMethodInfo() => implementationType.GetMethod(nameof(ICommandHandler<string>.ExecuteCommand), BindingFlags.Instance | BindingFlags.Public);
        }

        private static Func<IServiceProvider, CommandHandlerBehaviorInvocation<TCommand, TResponse>> CreateBehaviorInvocationFactory<TCommand, TResponse, TAttribute>(
            ServiceDescriptor descriptor, TAttribute attribute)
            where TCommand : notnull
            where TAttribute : Attribute
        {
            var genericBehaviorType = descriptor.ImplementationType!;
            var behaviorType = genericBehaviorType.MakeGenericType(typeof(TCommand), typeof(TResponse));
            var concreteDescriptor = ServiceDescriptor.Describe(behaviorType, behaviorType, descriptor.Lifetime);

            return p =>
            {
                var behaviorInstance = (ICommandHandlerBehavior<TCommand, TResponse, TAttribute>)p.GetInstance(concreteDescriptor);
                return (c, n, t) => behaviorInstance.InterceptCommand(c, n, attribute, t);
            };
        }

        private sealed class CommandHandlerServiceDescriptor : ServiceDescriptor
        {
            public CommandHandlerServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, Type originalHandlerType)
                : base(serviceType, implementationType, lifetime)
            {
                OriginalHandlerType = originalHandlerType;
            }

            public CommandHandlerServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, Type originalHandlerType)
                : base(serviceType, factory, lifetime)
            {
                OriginalHandlerType = originalHandlerType;
            }

            public Type OriginalHandlerType { get; }
        }

        private sealed class CommandHandlerBehaviorServiceDescriptor : ServiceDescriptor
        {
            public CommandHandlerBehaviorServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, Type markerAttributeType)
                : base(serviceType, implementationType, lifetime)
            {
                MarkerAttributeType = markerAttributeType;
            }

            public Type MarkerAttributeType { get; }
        }
    }
}
