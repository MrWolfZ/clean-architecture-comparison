using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Core.TestUtilities;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListStatisticsControllerTests : BaselineControllerTestBase
    {
        private ITaskListStatisticsRepository TaskListStatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GetStatistics_ReturnsStatistics()
        {
            var expectedResponse = new TaskListStatistics { NumberOfTaskListsCreated = 1 };

            await TaskListStatisticsRepository.Upsert(expectedResponse);

            var response = await HttpClient.GetFromJsonAsync<TaskListStatistics>("taskListStatistics");

            Assert.AreEqual(expectedResponse, response);
        }
    }
}
