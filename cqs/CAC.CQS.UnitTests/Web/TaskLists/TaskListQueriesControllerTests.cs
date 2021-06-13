using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.Core.TestUtilities;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListQueriesControllerTests : BaselineControllerTestBase
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GetAll_GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = CreateTaskList(numberOfEntries: 2);
            var taskList2 = CreateTaskList(numberOfEntries: 1);

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);

            var expectedResponse = GetAllTaskListsQueryResponse.FromTaskLists(new[] { taskList1, taskList2 });

            var response = await HttpClient.PostAsJsonAsync("taskLists/getAllTaskLists", new GetAllTaskListsQuery(), JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<GetAllTaskListsQueryResponse>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task GetById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            taskList = taskList.MarkEntryAsDone(taskList.Entries.First().Id);

            taskList = await TaskListRepository.Upsert(taskList);

            var expectedResponse = GetTaskListByIdQueryResponse.FromTaskList(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/getTaskListById", new GetTaskListByIdQuery { TaskListId = taskList.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<GetTaskListByIdQueryResponse>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);

            var response = await HttpClient.PostAsJsonAsync("taskLists/getTaskListById", new GetTaskListByIdQuery { TaskListId = nonExistingId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenExistingTaskLists_ReturnsTaskListsWithPendingEntries()
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

            var response = await HttpClient.PostAsJsonAsync("taskLists/getAllTaskListsWithPendingEntries", new GetAllTaskListsWithPendingEntriesQuery(), JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<GetAllTaskListsWithPendingEntriesQueryResponse>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);
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
