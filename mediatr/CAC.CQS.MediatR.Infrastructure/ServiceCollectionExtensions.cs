using System.Runtime.CompilerServices;
using CAC.Core.Infrastructure;
using CAC.CQS.MediatR.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.Users;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using CAC.CQS.MediatR.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.MediatR.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.MediatR.Infrastructure
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