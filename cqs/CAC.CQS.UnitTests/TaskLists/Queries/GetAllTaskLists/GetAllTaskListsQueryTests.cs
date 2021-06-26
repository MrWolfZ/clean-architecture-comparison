using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Queries.GetAllTaskLists
{
    public abstract class GetAllTaskListsQueryTests : QueryHandlingIntegrationTestBase<GetAllTaskListsQuery, GetAllTaskListsQueryResponse>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = CreateTaskList(numberOfEntries: 2);
            var taskList2 = CreateTaskList(numberOfEntries: 1);

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);

            var expectedResponse = GetAllTaskListsQueryResponse.FromTaskLists(new[] { taskList1, taskList2 });

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
