using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Core.Domain;
using CAC.Plain.Domain.TaskLists;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Plain.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Plain.Domain
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
