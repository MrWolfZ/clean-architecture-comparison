using System.Runtime.CompilerServices;
using CAC.Plain.Domain.TaskLists;
using CAC.Plain.Infrastructure.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Plain.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Plain.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ITaskListRepository, InMemoryTaskListRepository>();
        }
    }
}
