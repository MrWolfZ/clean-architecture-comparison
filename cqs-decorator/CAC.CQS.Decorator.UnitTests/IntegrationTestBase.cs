using CAC.CQS.Decorator.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using CAC.CQS.Decorator.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CAC.CQS.Decorator.UnitTests
{
    public abstract class IntegrationTestBase : Core.TestUtilities.CoreIntegrationTestBase
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
