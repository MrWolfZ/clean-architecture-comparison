using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CAC.Core.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDomainEventPublisher(this IServiceCollection services)
        {
            services.AddTransient<IDomainEventPublisher, DomainEventPublisher>();
        }

        public static void AddDomainEventHandler<TEventHandler>(this IServiceCollection services)
            where TEventHandler : class, IDomainEventHandler
        {
            var eventHandlerInterfaceTypes = typeof(TEventHandler).GetInterfaces().Where(IsEventHandlerInterface);

            foreach (var interfaceType in eventHandlerInterfaceTypes)
            {
                services.AddTransient(interfaceType, typeof(TEventHandler));
            }

            services.AddTransient<IDomainEventHandler, TEventHandler>();

            static bool IsEventHandlerInterface(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>);
        }

        public static void AddQueryHandler<TQueryHandler>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
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

            services.AddQueryHandler(queryHandlerInterface, typeof(TQueryHandler), serviceLifetime ?? ServiceLifetime.Transient);

            static bool IsQueryHandlerInterface(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
        }

        private static void AddQueryHandler(this IServiceCollection services, Type commandHandlerInterface, Type implementationType, ServiceLifetime serviceLifetime)
        {
            var queryType = commandHandlerInterface.GetGenericArguments().First();
            var responseType = commandHandlerInterface.GetGenericArguments().Skip(1).First();

            var descriptor = ServiceDescriptor.Describe(commandHandlerInterface, implementationType, serviceLifetime);

            descriptor = DecorateWithValidation(descriptor);
            descriptor = DecorateWithLogging(descriptor);

            services.Add(descriptor);

            ServiceDescriptor DecorateWithLogging(ServiceDescriptor d)
            {
                var attr = GetDecoratorMarkerAttribute<LogQueryAttribute>();

                if (attr == null)
                {
                    return d;
                }

                var decoratorType = typeof(QueryHandlerLoggingDecorator<,>).MakeGenericType(queryType, responseType);
                var options = new QueryHandlerLoggingOptions(attr.LogException);
                return d.Decorate(decoratorType, options);
            }

            ServiceDescriptor DecorateWithValidation(ServiceDescriptor d)
            {
                var attr = GetDecoratorMarkerAttribute<ValidateQueryAttribute>();
                var decoratorType = typeof(QueryHandlerValidationDecorator<,>).MakeGenericType(queryType, responseType);
                return attr == null ? d : d.Decorate(decoratorType);
            }

            TAttribute? GetDecoratorMarkerAttribute<TAttribute>()
                where TAttribute : Attribute
                => GetExecuteQueryMethodInfo()?.GetCustomAttribute<TAttribute>();

            MethodInfo? GetExecuteQueryMethodInfo() => implementationType.GetMethod(nameof(IQueryHandler<string, string>.ExecuteQuery), BindingFlags.Instance | BindingFlags.Public);
        }

        private static ServiceDescriptor Decorate(this ServiceDescriptor descriptor, Type decoratorType, params object[] extraArguments)
        {
            return descriptor.WithFactory(provider => provider.CreateDecoratorInstance(decoratorType, provider.GetInstance(descriptor), extraArguments));
        }

        private static ServiceDescriptor WithFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> factory)
        {
            return ServiceDescriptor.Describe(descriptor.ServiceType, factory, descriptor.Lifetime);
        }

        private static object CreateDecoratorInstance(this IServiceProvider provider, Type type, object decoratedInstance, params object[] extraArguments)
        {
            return ActivatorUtilities.CreateInstance(provider, type, extraArguments.Concat(new[] { decoratedInstance }).ToArray());
        }
    }
}
