using System.Runtime.CompilerServices;
using CAC.Basic.Application;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Application.Users;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Basic.Infrastructure.Users;
using CAC.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Basic.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Basic.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITaskListRepository, FileSystemTaskListRepository>();
            services.AddSingleton<ITaskListStatisticsRepository, InMemoryTaskListStatisticsRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddTransient<IMessageQueueAdapter, NullMessageQueueAdapter>();

            services.AddPersistenceOptions(configuration);
        }
    }
}
