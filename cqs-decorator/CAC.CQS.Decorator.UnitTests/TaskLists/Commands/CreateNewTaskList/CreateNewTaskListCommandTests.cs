using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.CreateNewTaskList
{
    public abstract class CreateNewTaskListCommandTests : CommandHandlingIntegrationTestBase<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        private static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenValidName_StoresTaskListAndReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListCommandResponse(1);

            const string name = "test";
            var response = await ExecuteCommand(new() { Name = name, OwnerId = PremiumOwner.Id });

            Assert.AreEqual(expectedResponse, response);

            var storedTaskList = await TaskListRepository.GetById(response.Id);

            Assert.AreEqual(name, storedTaskList?.Name);
        }

        [Test]
        public async Task GivenNonExistingOwnerId_FailsWithEntityNotFound()
        {
            await AssertCommandFailure(new() { Name = "test", OwnerId = 99 }, ExpectedCommandFailure.EntityNotFound);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task GivenInvalidName_FailsWithInvalidCommand(string name)
        {
            await AssertCommandFailure(new() { Name = name, OwnerId = PremiumOwner.Id }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenNameWithTooManyCharacters_FailsWithInvalidCommand()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", CreateNewTaskListCommand.MaxTaskListNameLength + 1));

            await AssertCommandFailure(new() { Name = name, OwnerId = PremiumOwner.Id }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenDuplicateNameForSameOwner_FailsWithDomainInvariantViolation()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { Name = taskList.Name, OwnerId = taskList.OwnerId }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenDuplicateNameForDifferentOwner_Succeeds()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { Name = taskList.Name, OwnerId = NonPremiumOwner.Id });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenPremiumOwnerWithExistingTaskList_Succeeds()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { Name = "new", OwnerId = taskList.OwnerId });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenNonPremiumOwnerWithExistingTaskList_FailsWithDomainInvariantViolation()
        {
            var taskList = CreateTaskList(NonPremiumOwner);

            _ = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { Name = "new", OwnerId = NonPremiumOwner.Id }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            _ = await ExecuteCommand(new() { Name = "test", OwnerId = PremiumOwner.Id });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsCreated);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var response = await ExecuteCommand(new() { Name = "test", OwnerId = PremiumOwner.Id });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListCreatedMessage>(m => m.TaskListId == response.Id)));
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
