using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.Core.TestUtilities;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.AddTaskToList;
using CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Decorator.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListCommandsIntegrationTests : IntegrationTestBase
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        private static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_StoresTaskListAndReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListCommandResponse(1);

            const string test = "test";
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = test, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<CreateNewTaskListCommandResponse>(JsonSerializerOptions);

            Assert.AreEqual(expectedResponse, responseContent);

            var storedTaskList = await TaskListRepository.GetById(responseContent!.Id);

            Assert.AreEqual(test, storedTaskList?.Name);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonExistingOwnerId_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = "test", OwnerId = 99 }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest(string name)
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNameWithTooManyCharacters_ReturnsBadRequest()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", CreateNewTaskListCommand.MaxTaskListNameLength + 1));

            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForSameOwner_ReturnsConflict()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = taskList.Name, OwnerId = taskList.OwnerId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForDifferentOwner_ReturnsSuccess()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = taskList.Name, OwnerId = NonPremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenPremiumOwnerWithExistingTaskList_ReturnsSuccess()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = "new", OwnerId = taskList.OwnerId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonPremiumOwnerWithExistingTaskList_ReturnsConflict()
        {
            var taskList = CreateTaskList(NonPremiumOwner);

            _ = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = "new", OwnerId = NonPremiumOwner.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_UpdatesStatistics()
        {
            _ = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = "test", OwnerId = PremiumOwner.Id }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsCreated);
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_PublishesNotification()
        {
            const string name = "test";
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", new CreateNewTaskListCommand { Name = name, OwnerId = PremiumOwner.Id }, JsonSerializerOptions);
            var deserialized = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CreateNewTaskListCommandResponse>(JsonSerializerOptions);
            var id = deserialized?.Id;

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListCreatedMessage>(m => m.TaskListId == id)));
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_UpdatesTaskListAndReturnsEntryId()
        {
            var expectedResponse = new AddTaskToListCommandResponse(1);
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            const string taskDescription = "task";
            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = taskDescription }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadFromJsonAsync<AddTaskToListCommandResponse>(JsonSerializerOptions);

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

            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = description }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_ReturnsBadRequest()
        {
            var taskList = CreateTaskList();
            var description = string.Join(string.Empty, Enumerable.Repeat("a", AddTaskToListCommand.MaxTaskDescriptionLength + 1));

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = description }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = nonExistingId, TaskDescription = "task" }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithLessThanFiveEntriesAndNonPremiumOwner_ReturnsSuccess()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 4);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = "new" }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithFiveEntriesAndNonPremiumOwner_ReturnsConflict()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 5);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = "new" }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = "task" }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", new AddTaskToListCommand { TaskListId = taskList.Id, TaskDescription = "task" }, JsonSerializerOptions);

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskAddedToListMessage>(m => m.TaskListId == taskList.Id)));
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidEntryId_UpdatesTaskListAndReturnsNoContent()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", new MarkTaskAsDoneCommand { TaskListId = taskList.Id, EntryId = entryId }, JsonSerializerOptions);

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

            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", new MarkTaskAsDoneCommand { TaskListId = taskList.Id, EntryId = nonExistingEntryId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var entryId = TaskListEntryId.Of(1);

            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", new MarkTaskAsDoneCommand { TaskListId = nonExistingId, EntryId = entryId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList(numberOfEntries: 1);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", new MarkTaskAsDoneCommand { TaskListId = taskList.Id, EntryId = entryId }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, 1, "task");
            taskList = taskList.AddEntry(newEntry);

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", new MarkTaskAsDoneCommand { TaskListId = taskList.Id, EntryId = newEntry.Id }, JsonSerializerOptions);

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskMarkedAsDoneMessage>(m => m.TaskListId == taskList.Id && m.TaskListEntryId == newEntry.Id)));
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskListId_DeletesTaskListAndReturnsNoContent()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists/deleteTaskList", new DeleteTaskListCommand { TaskListId = taskList.Id }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var foundTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsNull(foundTaskList);
        }

        [Test]
        public async Task DeleteById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var response = await HttpClient.PostAsJsonAsync("taskLists/deleteTaskList", new DeleteTaskListCommand { TaskListId = nonExistingId }, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/deleteTaskList", new DeleteTaskListCommand { TaskListId = taskList.Id }, JsonSerializerOptions);

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsDeleted);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await HttpClient.PostAsJsonAsync("taskLists/deleteTaskList", new DeleteTaskListCommand { TaskListId = taskList.Id }, JsonSerializerOptions);

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListDeletedMessage>(m => m.TaskListId == taskList.Id)));
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
