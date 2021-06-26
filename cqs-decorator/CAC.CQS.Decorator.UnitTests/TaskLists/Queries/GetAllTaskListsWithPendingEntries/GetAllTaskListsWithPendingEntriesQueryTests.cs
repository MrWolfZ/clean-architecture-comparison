using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Queries.GetAllTaskListsWithPendingEntries
{
    public abstract class GetAllTaskListsWithPendingEntriesQueryTests : QueryHandlingIntegrationTestBase<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskLists_ReturnsTaskListsWithPendingEntries()
        {
            var taskList1 = CreateTaskList(numberOfEntries: 2);
            taskList1 = taskList1.MarkEntryAsDone(taskList1.Entries.First().Id);

            var taskList2 = CreateTaskList(numberOfEntries: 1);

            var taskList3 = CreateTaskList(numberOfEntries: 1);
            taskList3 = taskList3.MarkEntryAsDone(taskList3.Entries.First().Id);

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);
            _ = await TaskListRepository.Upsert(taskList3);

            var expectedResponse = GetAllTaskListsWithPendingEntriesQueryResponse.FromTaskLists(new[] { taskList1, taskList2 });

            var response = await ExecuteQuery(new());

            Assert.AreEqual(expectedResponse, response);
        }

        private TaskList CreateTaskList(User? owner = null, int numberOfEntries = 0)
        {
            var listId = ++taskListIdCounter;
            var entries = Enumerable.Range(1, numberOfEntries).Select(_ => CreateEntry()).ToValueList();
            return TaskList.FromRawData(listId, (owner ?? PremiumOwner).Id, (owner ?? PremiumOwner).IsPremium, $"list {listId}", entries, SystemTime.Now, null);
        }

        private TaskListEntry CreateEntry()
        {
            var entryId = ++taskListEntryIdCounter;
            return TaskListEntry.FromRawData(entryId, $"task {entryId}", false);
        }
    }
}
