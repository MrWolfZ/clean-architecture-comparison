using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Core.Domain;
using CAC.DDD.Domain.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.DDD.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.DDD.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDomain(this IServiceCollection services)
        {
            services.AddTransient<ITaskListService, TaskListService>();
            
            Assembly.GetExecutingAssembly().AddTypeConverterAttributes();
        }
    }
}
