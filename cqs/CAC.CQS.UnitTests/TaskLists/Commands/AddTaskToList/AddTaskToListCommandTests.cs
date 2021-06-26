using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.AddTaskToList;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Commands.AddTaskToList
{
    public abstract class AddTaskToListCommandTests : CommandHandlingIntegrationTestBase<AddTaskToListCommand, AddTaskToListCommandResponse>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        private static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListIdAndValidDescription_UpdatesTaskListAndReturnsEntryId()
        {
            var expectedResponse = new AddTaskToListCommandResponse(1);
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            const string taskDescription = "task";
            var response = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = taskDescription });

            Assert.AreEqual(expectedResponse, response);

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.AreEqual(taskDescription, storedTaskList?.Entries.Single().Description);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task GivenExistingTaskListIdAndInvalidDescription_FailsWithInvalidCommand(string description)
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, TaskDescription = description }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_FailsWithInvalidCommand()
        {
            var taskList = CreateTaskList();
            var description = string.Join(string.Empty, Enumerable.Repeat("a", AddTaskToListCommand.MaxTaskDescriptionLength + 1));

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, TaskDescription = description }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenNonExistingTaskListId_FailsWithEntityNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            await AssertCommandFailure(new() { TaskListId = nonExistingId, TaskDescription = "task" }, ExpectedCommandFailure.EntityNotFound);
        }

        [Test]
        public async Task GivenTaskListWithLessThanFiveEntriesAndNonPremiumOwner_Succeeds()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 4);

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "new" });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenTaskListWithFiveEntriesAndNonPremiumOwner_FailsWithDomainInvariantViolation()
        {
            var taskList = CreateTaskList(NonPremiumOwner, 5);

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, TaskDescription = "new" }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);
            
            _ = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "task" });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "task" });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskAddedToListMessage>(m => m.TaskListId == taskList.Id)));
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
