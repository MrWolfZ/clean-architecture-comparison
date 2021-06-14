using System.Threading.Tasks;
using CAC.Core.Jobs;
using CAC.CQS.MediatR.Application;
using CAC.CQS.MediatR.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CAC.CQS.MediatR.Jobs
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

            services.AddJob<SendTaskListRemindersJob>();
        }
    }
}
