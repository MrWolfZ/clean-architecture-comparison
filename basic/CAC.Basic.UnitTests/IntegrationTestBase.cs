using CAC.Basic.Application;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Basic.Web;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CAC.Basic.UnitTests
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
