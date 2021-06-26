using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using CAC.CQS.MediatR.UnitTests.Domain.TaskListAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.MarkTaskAsDone
{
    public abstract class MarkTaskAsDoneCommandTests : CommandHandlingIntegrationTestBase<MarkTaskAsDoneCommand>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListIdAndValidEntryId_UpdatesTaskList()
        {
            var taskList = new TaskListBuilder().WithPendingEntries(2).Build();
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = entryId });

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsTrue(storedTaskList?.Entries.Single(e => e.Id == entryId).IsDone);
        }

        [Test]
        public async Task GivenExistingTaskListIdAndNonExistingEntryId_FailsWithEntityNotFound()
        {
            var taskList = new TaskListBuilder().WithPendingEntries(2).Build();
            var nonExistingEntryId = TaskListEntryId.Of(99);

            taskList = await TaskListRepository.Upsert(taskList);

            await AssertCommandFailure(new() { TaskListId = taskList.Id, EntryId = nonExistingEntryId }, ExpectedCommandFailure.EntityNotFound);
        }

        [Test]
        public async Task GivenNonExistingTaskListId_FailsWithEntityNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            var entryId = TaskListEntryId.Of(1);

            await AssertCommandFailure(new() { TaskListId = nonExistingId, EntryId = entryId }, ExpectedCommandFailure.EntityNotFound);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskListBuilder().WithPendingEntries(1).Build();
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = entryId });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = new TaskListBuilder().Build();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, 1, "task");
            taskList = taskList.AddEntry(newEntry);

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = newEntry.Id });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskMarkedAsDoneMessage>(m => m.TaskListId == taskList.Id && m.TaskListEntryId == newEntry.Id)));
        }
    }
}
