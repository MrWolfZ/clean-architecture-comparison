using System;
using System.Linq;
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

        public static void AddCommandHandler<TCommandHandler>(this IServiceCollection services)
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

            services.AddTransient(commandHandlerInterfaces.Single(), typeof(TCommandHandler));

            static bool IsCommandHandlerInterface(Type i) =>
                i.IsGenericType
                && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
        }

        public static void AddQueryHandler<TQueryHandler>(this IServiceCollection services)
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
            
            services.AddTransient(queryHandlerInterfaces.Single(), typeof(TQueryHandler));

            static bool IsQueryHandlerInterface(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
        }
    }
}
