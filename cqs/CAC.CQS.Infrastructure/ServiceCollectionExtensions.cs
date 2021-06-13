using System.Runtime.CompilerServices;
using CAC.Core.Infrastructure;
using CAC.CQS.Application;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.Users;
using CAC.CQS.Infrastructure.TaskLists;
using CAC.CQS.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Infrastructure
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