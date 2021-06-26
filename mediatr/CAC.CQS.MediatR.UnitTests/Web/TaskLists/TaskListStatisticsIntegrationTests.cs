using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.TestUtilities;
using CAC.CQS.MediatR.Application.TaskLists;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListStatisticsIntegrationTests : IntegrationTestBase
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