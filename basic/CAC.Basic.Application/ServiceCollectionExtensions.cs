using System.Runtime.CompilerServices;
using CAC.Basic.Application.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Basic.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Basic.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDomain(this IServiceCollection services)
        {
            services.AddTransient<ITaskListService, TaskListService>();
            services.AddSingleton<ITaskListStatisticsService, InMemoryTaskListStatisticsService>();
        }
    }
}
