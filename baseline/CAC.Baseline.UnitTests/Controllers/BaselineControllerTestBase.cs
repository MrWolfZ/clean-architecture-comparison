﻿using CAC.Baseline.Web;
using CAC.Baseline.Web.Persistence;
using CAC.Baseline.Web.Services;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CAC.Baseline.UnitTests.Controllers
{
    public abstract class BaselineControllerTestBase : ControllerTestBase
    {
        protected Mock<IMessageQueueAdapter> MessageQueueAdapterMock { get; } = new Mock<IMessageQueueAdapter>();

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<ITaskListRepository, InMemoryTaskListRepository>());
            services.Replace(ServiceDescriptor.Singleton<ITaskListEntryRepository, InMemoryTaskListEntryRepository>());
            services.Replace(ServiceDescriptor.Singleton(MessageQueueAdapterMock.Object));
        }
    }
}
