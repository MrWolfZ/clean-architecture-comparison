using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Queries.GetTaskListById
{
    public abstract class GetTaskListByIdQueryTests : QueryHandlingIntegrationTestBase<GetTaskListByIdQuery, GetTaskListByIdQueryResponse>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
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
