using System.Runtime.CompilerServices;
using CAC.CQS.Domain.TaskLists;
using CAC.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ITaskListRepository, InMemoryTaskListRepository>();
        }
    }
}
