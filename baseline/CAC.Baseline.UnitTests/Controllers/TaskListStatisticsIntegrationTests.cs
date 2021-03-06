using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Services;
using CAC.Core.TestUtilities;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Controllers
{
    [IntegrationTest]
    public sealed class TaskListStatisticsIntegrationTests : IntegrationTestBase
    {
        private ITaskListStatisticsService StatisticsService => Resolve<ITaskListStatisticsService>();

        [Test]
        public async Task GetStatistics_ReturnsStatistics()
        {
            var expectedResponse = new TaskListStatistics { NumberOfTaskListsCreated = 1 };

            await StatisticsService.OnTaskListCreated(new(1, 1, "test"));

            var response = await HttpClient.GetFromJsonAsync<TaskListStatistics>("taskListStatistics");

            Assert.AreEqual(expectedResponse, response);
        }
    }
}
