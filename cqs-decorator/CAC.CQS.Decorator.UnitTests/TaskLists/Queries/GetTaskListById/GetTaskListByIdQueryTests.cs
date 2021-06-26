using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.UnitTests.Domain.TaskListAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Queries.GetTaskListById
{
    public abstract class GetTaskListByIdQueryTests : QueryHandlingIntegrationTestBase<GetTaskListByIdQuery, GetTaskListByIdQueryResponse>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = new TaskListBuilder().WithPendingEntries(2).Build();
            taskList = taskList.MarkEntryAsDone(taskList.Entries.First().Id);

            taskList = await TaskListRepository.Upsert(taskList);

            var expectedResponse = GetTaskListByIdQueryResponse.FromTaskList(taskList);

            var response = await ExecuteQuery(new() { TaskListId = taskList.Id });

            Assert.AreEqual(expectedResponse, response);
        }

        [Test]
        public async Task GivenNonExistingTaskListId_FailsWithEntityNotFound()
        {
            var nonExistingId = TaskListId.Of(1);

            await AssertQueryFailure(new() { TaskListId = nonExistingId }, ExpectedQueryFailure.EntityNotFound);
        }
    }
}
