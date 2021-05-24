using System.Runtime.CompilerServices;
using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Plain.CQS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Plain.CQS.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ITaskListRepository, InMemoryTaskListRepository>();
        }
    }
}
