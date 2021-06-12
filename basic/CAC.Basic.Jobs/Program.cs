using System.Threading.Tasks;
using CAC.Basic.Application;
using CAC.Basic.Infrastructure;
using CAC.Core.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CAC.Basic.Jobs
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureServices(ConfigureServices)
                      .RunConsoleAsync();
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure(hostContext.Configuration);
            services.AddJobTriggerService(hostContext.Configuration);
        }
    }
}
