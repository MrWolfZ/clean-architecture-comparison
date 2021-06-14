using CAC.Core.TestUtilities;
using CAC.CQS.MediatR.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using CAC.CQS.MediatR.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CAC.CQS.MediatR.UnitTests.Web.TaskLists
{
    public abstract class BaselineControllerTestBase : ControllerTestBase
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