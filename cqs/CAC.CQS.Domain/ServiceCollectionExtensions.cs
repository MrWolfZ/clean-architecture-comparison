using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Core.Domain;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDomain(this IServiceCollection services)
        {
            services.AddCommandHandlers();
            services.AddQueryHandlers();

            Assembly.GetExecutingAssembly().AddEntityIdTypeConverterAttributes();
        }

        private static void AddCommandHandlers(this IServiceCollection services)
        {
            foreach (var commandHandlerType in Assembly.GetExecutingAssembly().GetTypes().Where(IsCommandHandlerType))
            {
                services.AddCommandHandler(commandHandlerType);
            }
        }

        private static void AddCommandHandler(this IServiceCollection services, Type commandHandlerType)
        {
            services.AddTransient(GetCommandHandlerInterface(commandHandlerType)!, commandHandlerType);
            services.AddTransient(commandHandlerType);
        }

        private static bool IsCommandHandlerType(Type type) => GetCommandHandlerInterface(type) != null;

        private static Type? GetCommandHandlerInterface(Type type)
        {
            var commandHandlerInterfaces = type.GetInterfaces().Where(IsCommandHandlerInterface).ToList();

            if (commandHandlerInterfaces.Count > 1)
            {
                throw new ArgumentException($"type {type.Name} implements more than one command handler interface", nameof(type));
            }

            return commandHandlerInterfaces.FirstOrDefault();

            static bool IsCommandHandlerInterface(Type t) =>
                t.IsGenericType
                && (t.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    || t.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
        }

        private static void AddQueryHandlers(this IServiceCollection services)
        {
            foreach (var queryHandlerType in Assembly.GetExecutingAssembly().GetTypes().Where(IsQueryHandlerType))
            {
                services.AddQueryHandler(queryHandlerType);
            }
        }

        private static void AddQueryHandler(this IServiceCollection services, Type queryHandlerType)
        {
            services.AddTransient(GetQueryHandlerInterface(queryHandlerType)!, queryHandlerType);
            services.AddTransient(queryHandlerType);
        }

        private static bool IsQueryHandlerType(Type type) => GetQueryHandlerInterface(type) != null;

        private static Type? GetQueryHandlerInterface(Type type)
        {
            var queryHandlerInterfaces = type.GetInterfaces().Where(IsQueryHandlerInterface).ToList();

            if (queryHandlerInterfaces.Count > 1)
            {
                throw new ArgumentException($"type {type.Name} implements more than one command handler interface", nameof(type));
            }

            return queryHandlerInterfaces.FirstOrDefault();

            static bool IsQueryHandlerInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
        }
    }
}
