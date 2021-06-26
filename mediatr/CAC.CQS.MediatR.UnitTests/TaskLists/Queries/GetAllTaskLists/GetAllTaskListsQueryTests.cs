using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.MediatR.UnitTests.Domain.TaskListAggregate;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Queries.GetAllTaskLists
{
    public abstract class GetAllTaskListsQueryTests : QueryHandlingIntegrationTestBase<GetAllTaskListsQuery, GetAllTaskListsQueryResponse>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = new TaskListBuilder().WithPendingEntries(2).Build();
            var taskList2 = new TaskListBuilder().WithPendingEntries(1).Build();

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);

            var expectedResponse = GetAllTaskListsQueryResponse.FromTaskLists(new[] { taskList1, taskList2 });

            var response = await ExecuteQuery(new());

            Assert.AreEqual(expectedResponse, response);
        }
    }
}
