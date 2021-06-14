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

        public static void AddCommandHandler<TCommandHandler>(this IServiceCollection services, ServiceLifetime? serviceLifetime = null)
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

            services.AddCommandHandler(commandHandlerInterface, typeof(TCommandHandler), serviceLifetime ?? ServiceLifetime.Transient);

            static bool IsCommandHandlerInterface(Type i) =>
                i.IsGenericType
                && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
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

        private static void AddCommandHandler(this IServiceCollection services, Type commandHandlerInterface, Type implementationType, ServiceLifetime serviceLifetime)
        {
            Type commandType = commandHandlerInterface.GetGenericArguments().First();
            var responseType = commandHandlerInterface.GetGenericArguments().Skip(1).FirstOrDefault();

            var descriptor = ServiceDescriptor.Describe(commandHandlerInterface, implementationType, serviceLifetime);

            descriptor = DecorateWithValidation(descriptor);
            descriptor = DecorateWithLogging(descriptor);

            services.Add(descriptor);

            ServiceDescriptor DecorateWithLogging(ServiceDescriptor d)
            {
                var attr = GetDecoratorMarkerAttribute<LogCommandAttribute>();

                if (attr == null)
                {
                    return d;
                }

                var decoratorType = responseType == null
                    ? typeof(CommandHandlerLoggingDecorator<>).MakeGenericType(commandType)
                    : typeof(CommandHandlerLoggingDecorator<,>).MakeGenericType(commandType, responseType);
                var options = new CommandHandlerLoggingOptions(attr.LogException);
                return d.Decorate(decoratorType, options);
            }

            ServiceDescriptor DecorateWithValidation(ServiceDescriptor d)
            {
                var attr = GetDecoratorMarkerAttribute<ValidateCommandAttribute>();
                var decoratorType = responseType == null
                    ? typeof(CommandHandlerValidationDecorator<>).MakeGenericType(commandType)
                    : typeof(CommandHandlerValidationDecorator<,>).MakeGenericType(commandType, responseType);
                return attr == null ? d : d.Decorate(decoratorType);
            }

            TAttribute? GetDecoratorMarkerAttribute<TAttribute>()
                where TAttribute : Attribute
                => GetExecuteCommandMethodInfo()?.GetCustomAttribute<TAttribute>();

            MethodInfo? GetExecuteCommandMethodInfo() => implementationType.GetMethod(nameof(ICommandHandler<string>.ExecuteCommand), BindingFlags.Instance | BindingFlags.Public);
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

        private static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType != null)
            {
                return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(provider);
            }

            throw new InvalidOperationException("cannot instantiate descriptor");
        }

        private static ServiceDescriptor Decorate(this ServiceDescriptor descriptor, Type decoratorType, params object[] extraArguments)
        {
            return descriptor.WithFactory(provider => provider.CreateDecoratorInstance(decoratorType, provider.GetInstance(descriptor), extraArguments));
        }

        private static ServiceDescriptor WithFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> factory)
        {
            return ServiceDescriptor.Describe(descriptor.ServiceType, factory, descriptor.Lifetime);
        }

        private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }

        private static object CreateDecoratorInstance(this IServiceProvider provider, Type type, object decoratedInstance, params object[] extraArguments)
        {
            return ActivatorUtilities.CreateInstance(provider, type, extraArguments.Concat(new[] { decoratedInstance }).ToArray());
        }
    }
}
