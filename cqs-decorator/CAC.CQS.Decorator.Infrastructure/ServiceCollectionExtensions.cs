using System.Runtime.CompilerServices;
using CAC.Core.Infrastructure;
using CAC.CQS.Decorator.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.Users;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using CAC.CQS.Decorator.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.Decorator.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Decorator.Infrastructure
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