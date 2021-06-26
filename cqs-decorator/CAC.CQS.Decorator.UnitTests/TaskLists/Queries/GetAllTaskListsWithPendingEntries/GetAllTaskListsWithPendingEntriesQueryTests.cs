using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Decorator.UnitTests.Domain.TaskListAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Queries.GetAllTaskListsWithPendingEntries
{
    public abstract class GetAllTaskListsWithPendingEntriesQueryTests : QueryHandlingIntegrationTestBase<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskLists_ReturnsTaskListsWithPendingEntries()
        {
            var taskList1 = new TaskListBuilder().WithPendingEntries(2).Build();
            taskList1 = taskList1.MarkEntryAsDone(taskList1.Entries.First().Id);

            var taskList2 = new TaskListBuilder().WithPendingEntries(1).Build();

            var taskList3 = new TaskListBuilder().WithPendingEntries(1).Build();
            taskList3 = taskList3.MarkEntryAsDone(taskList3.Entries.First().Id);

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);
            _ = await TaskListRepository.Upsert(taskList3);

            var expectedResponse = GetAllTaskListsWithPendingEntriesQueryResponse.FromTaskLists(new[] { taskList1, taskList2 });

            var response = await ExecuteQuery(new());

            Assert.AreEqual(expectedResponse, response);
        }
    }
}
