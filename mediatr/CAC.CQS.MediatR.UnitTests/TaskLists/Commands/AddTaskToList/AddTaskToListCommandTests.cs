using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.AddTaskToList;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using CAC.CQS.MediatR.UnitTests.Domain.TaskListAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.AddTaskToList
{
    public abstract class AddTaskToListCommandTests : CommandHandlingIntegrationTestBase<AddTaskToListCommand, AddTaskToListCommandResponse>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListIdAndValidDescription_UpdatesTaskListAndReturnsEntryId()
        {
            var expectedResponse = new AddTaskToListCommandResponse(1);
            var taskList = new TaskListBuilder().Build();

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
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, TaskDescription = description }, ExpectedCommandFailure.InvalidCommand);
        }

        [Test]
        public async Task GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_FailsWithInvalidCommand()
        {
            var taskList = new TaskListBuilder().Build();
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
            var taskList = new TaskListBuilder().WithNonPremiumOwner().WithPendingEntries(4).Build();

            taskList = await TaskListRepository.Upsert(taskList);

            var response = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "new" });

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GivenTaskListWithFiveEntriesAndNonPremiumOwner_FailsWithDomainInvariantViolation()
        {
            var taskList = new TaskListBuilder().WithNonPremiumOwner().WithPendingEntries(5).Build();

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, TaskDescription = "new" }, ExpectedCommandFailure.DomainInvariantViolation);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "task" });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            _ = await ExecuteCommand(new() { TaskListId = taskList.Id, TaskDescription = "task" });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskAddedToListMessage>(m => m.TaskListId == taskList.Id)));
        }
    }
}
