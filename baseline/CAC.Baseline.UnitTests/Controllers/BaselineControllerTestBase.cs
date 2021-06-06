using CAC.Baseline.Web;
using CAC.Baseline.Web.Persistence;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CAC.Baseline.UnitTests.Controllers
{
    public abstract class BaselineControllerTestBase : ControllerTestBase
    {
        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<ITaskListRepository, InMemoryTaskListRepository>());
            services.Replace(ServiceDescriptor.Singleton<ITaskListEntryRepository, InMemoryTaskListEntryRepository>());
        }
    }
}
