using System.Runtime.CompilerServices;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Core.Application;
using CAC.Core.Domain;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Basic.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Basic.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddTransient<ITaskListService, TaskListService>();
            
            services.AddDomainEventPublisher();
            services.AddDomainEventHandler<TaskListStatisticsDomainEventHandler>();
            services.AddDomainEventHandler<TaskListNotificationDomainEventHandler>();
            
            typeof(TaskListId).Assembly.AddEntityIdTypeConverterAttributes();
        }
    }
}
