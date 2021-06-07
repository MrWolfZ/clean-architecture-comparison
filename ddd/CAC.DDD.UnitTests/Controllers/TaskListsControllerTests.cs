using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.TestUtilities;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;
using CAC.DDD.Web.Dtos;
using CAC.DDD.Web.Persistence;
using CAC.DDD.Web.Services;
using Moq;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Controllers
{
    [IntegrationTest]
    public sealed class TaskListsControllerTests : BaselineControllerTestBase
    {
        private static readonly User PremiumOwner = User.New(1, "premium", true);
        private static readonly User NonPremiumOwner = User.New(2, "non-premium", false);

        private long taskListIdCounter;
        private long taskListEntryIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_StoresTaskListAndReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListResponseDto(1);

            const string test = "test";
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = test, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<CreateNewTaskListResponseDto>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);

            var storedTaskList = await TaskListRepository.GetById(responseContent!.Id);

            Assert.AreEqual(test, storedTaskList?.Name);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonExistingOwnerId_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test", OwnerId = 99 }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest(string name)
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNameWithTooManyCharacters_ReturnsBadRequest()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", CreateNewTaskListRequestDto.MaxTaskListNameLength + 1));

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForSameOwner_ReturnsConflict()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = taskList.OwnerId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForDifferentOwner_ReturnsSuccess()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = NonPremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenPremiumOwnerWithExistingTaskList_ReturnsSuccess()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = taskList.OwnerId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonPremiumOwnerWithExistingTaskList_ReturnsConflict()
        {
            var taskList = CreateTaskList(NonPremiumOwner);

            _ = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = NonPremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_UpdatesStatistics()
        {
            _ = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test", OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsCreated);
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_PublishesNotification()
        {
            const string name = "test";
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);
            var deserialized = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CreateNewTaskListResponseDto>(JsonSerializerOptions);
            var id = deserialized?.Id;

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListCreatedMessage>(m => m.TaskListId == id)));
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_UpdatesTaskListAndReturnsEntryId()
        {
            var expectedResponse = new AddTaskToListResponseDto(1);
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            const string taskDescription = "task";
            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = taskDescription }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<AddTaskToListResponseDto>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.AreEqual(taskDescription, storedTaskList?.Entries.Single().Description);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest(string description)
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = description }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_ReturnsBadRequest()
        {
            var taskList = CreateTaskList();
            var description = string.Join(string.Empty, Enumerable.Repeat("a", AddTaskToListRequestDto.MaxTaskDescriptionLength + 1));

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = description }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var response = await HttpClient.PostAsJsonAsync($"taskLists/{nonExistingId}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithLessThanFiveEntriesAndNonPremiumOwner_ReturnsSuccess()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 4);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithFiveEntriesAndNonPremiumOwner_ReturnsConflict()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 5);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" }, JsonSerializerOptions);

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskAddedToListMessage>(m => m.TaskListId == taskList.Id)));
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidEntryId_UpdatesTaskListAndReturnsNoContent()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{entryId}/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsTrue(storedTaskList?.Entries.Single(e => e.Id == entryId).IsDone);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingEntryId_ReturnsNotFound()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            var nonExistingEntryId = TaskListEntryId.Of(99);

            taskList = await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{nonExistingEntryId}/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var entryId = TaskListEntryId.Of(1);
            
            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{nonExistingId}/tasks/{entryId}/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList(numberOfEntries: 1);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            _ = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{entryId}/isDone", content);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();
            var entryId = TaskListEntryId.Of(1);
            taskList = taskList.AddEntry(entryId, "task");

            taskList = await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            _ = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{entryId}/isDone", content);

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskMarkedAsDoneMessage>(m => m.TaskListId == taskList.Id && m.TaskListEntryId == entryId)));
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskListId_DeletesTaskListAndReturnsNoContent()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.DeleteAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var foundTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsNull(foundTaskList);
        }

        [Test]
        public async Task DeleteById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var response = await HttpClient.DeleteAsync($"taskLists/{nonExistingId}");

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.DeleteAsync($"taskLists/{taskList.Id}");

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsDeleted);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.DeleteAsync($"taskLists/{taskList.Id}");

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListDeletedMessage>(m => m.TaskListId == taskList.Id)));
        }

        [Test]
        public async Task GetAll_GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = CreateTaskList(numberOfEntries: 2);
            var taskList2 = CreateTaskList(numberOfEntries: 1);

            taskList1 = await TaskListRepository.Upsert(taskList1);
            taskList2 = await TaskListRepository.Upsert(taskList2);

            var response = await HttpClient.GetAsync("taskLists");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var lists = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TaskListDto>>(JsonSerializerOptions);

            Assert.AreEqual(2, lists?.Count);
            Assert.IsTrue(lists?.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists?.Any(l => l.Name == taskList2.Name));
        }

        [Test]
        public async Task GetById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            taskList = taskList.MarkEntryAsDone(taskList.Entries.First().Id);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadFromJsonAsync<TaskListDto>(JsonSerializerOptions);

            Assert.AreEqual(taskList.Name, responseContent!.Name);
            Assert.IsTrue(taskList.Entries.Select(TaskListEntryDto.FromTaskListEntry).SequenceEqual(responseContent.Entries));
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var response = await HttpClient.GetAsync($"taskLists/{nonExistingId}");

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

            var response = await HttpClient.GetAsync("taskLists/withPendingEntries");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var lists = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TaskListDto>>(JsonSerializerOptions);

            Assert.AreEqual(2, lists?.Count);
            Assert.IsTrue(lists?.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists?.Any(l => l.Name == taskList2.Name));
        }

        private TaskList CreateTaskList(User? owner = null, int numberOfEntries = 0)
        {
            var listId = ++taskListIdCounter;
            var entries = Enumerable.Range(1, numberOfEntries).Select(id => CreateEntry(id)).ToValueList();
            return TaskList.New(listId, (owner ?? PremiumOwner).Id, $"list {listId}", entries);
        }

        private TaskListEntry CreateEntry(TaskListId owningListId)
        {
            var entryId = ++taskListEntryIdCounter;
            return TaskListEntry.New(owningListId, entryId, $"task {entryId}", false);
        }
    }
}
