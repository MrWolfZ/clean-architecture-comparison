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
    }
}
