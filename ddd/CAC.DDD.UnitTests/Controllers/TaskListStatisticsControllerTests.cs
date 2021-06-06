using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.TestUtilities;
using CAC.DDD.Web.Domain;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Services;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Controllers
{
    [IntegrationTest]
    public sealed class TaskListStatisticsControllerTests : BaselineControllerTestBase
    {
        private ITaskListStatisticsService StatisticsService => Resolve<ITaskListStatisticsService>();

        [Test]
        public async Task GetStatistics_ReturnsStatistics()
        {
            var expectedResponse = new TaskListStatistics { NumberOfTaskListsCreated = 1 };

            await StatisticsService.OnTaskListCreated(TaskList.New(1, 1, "test", ValueList<TaskListEntry>.Empty));

            var response = await HttpClient.GetFromJsonAsync<TaskListStatistics>("taskListStatistics");

            Assert.AreEqual(expectedResponse, response);
        }
    }
}
