using CAC.Core.TestUtilities;
using CAC.DDD.Web;
using CAC.DDD.Web.Persistence;
using CAC.DDD.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CAC.DDD.UnitTests.Controllers
{
    public abstract class IntegrationTestBase : CoreIntegrationTestBase
    {
        protected Mock<IMessageQueueAdapter> MessageQueueAdapterMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            _ = webHost.UseStartup<Startup>();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            _ = services.Replace(ServiceDescriptor.Singleton<ITaskListRepository, InMemoryTaskListRepository>())
                        .Replace(ServiceDescriptor.Singleton(MessageQueueAdapterMock.Object));
        }
    }
}
