using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Domain.TaskLists;
using CAC.Basic.Web;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListStatisticsControllerTests : ControllerTestBase
    {
        private ITaskListStatisticsService StatisticsService => Resolve<ITaskListStatisticsService>();

        [Test]
        public async Task GetStatistics_ReturnsStatistics()
        {
            var expectedResponse = new TaskListStatistics { NumberOfTaskListsCreated = 1 };

            await StatisticsService.OnTaskListCreated(new TaskList(1, 1, "test"));

            var response = await HttpClient.GetFromJsonAsync<TaskListStatistics>("taskListStatistics");

            Assert.AreEqual(expectedResponse, response);
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }
    }
}
