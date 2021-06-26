using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Decorator.UnitTests.Domain.TaskListAggregate;
using CAC.CQS.Decorator.UnitTests.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.CreateNewTaskList
{
    public abstract class CreateNewTaskListCommandTests : CommandHandlingIntegrationTestBase<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenValidName_StoresTaskListAndReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListCommandResponse(1);

            const string name = "test";
            var response = await ExecuteCommand(new() { Name = name, OwnerId = UserBuilder.PremiumOwner.Id });

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
            await AssertCommandFailure(new() { Name = name, OwnerId = UserBuilder.PremiumOwner.Id }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenNameWithTooManyCharacters_FailsWithInvalidCommand()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", CreateNewTaskListCommand.MaxTaskListNameLength + 1));

            await AssertCommandFailure(new() { Name = name, OwnerId = UserBuilder.PremiumOwner.Id }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenDuplicateNameForSameOwner_FailsWithDomainInvariantViolation()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { Name = taskList.Name, OwnerId = taskList.OwnerId }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenDuplicateNameForDifferentOwner_Succeeds()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { Name = taskList.Name, OwnerId = UserBuilder.NonPremiumOwner.Id });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenPremiumOwnerWithExistingTaskList_Succeeds()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { Name = "new", OwnerId = taskList.OwnerId });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenNonPremiumOwnerWithExistingTaskList_FailsWithDomainInvariantViolation()
        {
            var taskList = new TaskListBuilder().WithNonPremiumOwner().Build();

            _ = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { Name = "new", OwnerId = UserBuilder.NonPremiumOwner.Id }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            _ = await ExecuteCommand(new() { Name = "test", OwnerId = UserBuilder.PremiumOwner.Id });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsCreated);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var response = await ExecuteCommand(new() { Name = "test", OwnerId = UserBuilder.PremiumOwner.Id });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListCreatedMessage>(m => m.TaskListId == response.Id)));
        }
    }
}
