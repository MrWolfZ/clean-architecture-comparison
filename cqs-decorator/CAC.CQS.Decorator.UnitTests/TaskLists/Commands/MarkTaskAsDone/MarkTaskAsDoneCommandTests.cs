using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.MarkTaskAsDone
{
    public abstract class MarkTaskAsDoneCommandTests : CommandHandlingIntegrationTestBase<MarkTaskAsDoneCommand>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListIdAndValidEntryId_UpdatesTaskList()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = entryId });

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsTrue(storedTaskList?.Entries.Single(e => e.Id == entryId).IsDone);
        }

        [Test]
        public async Task GivenExistingTaskListIdAndNonExistingEntryId_FailsWithEntityNotFound()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);
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
            var taskList = CreateTaskList(numberOfEntries: 1);
            var entryId = taskList.Entries.First().Id;

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = entryId });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, 1, "task");
            taskList = taskList.AddEntry(newEntry);

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id, EntryId = newEntry.Id });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskMarkedAsDoneMessage>(m => m.TaskListId == taskList.Id && m.TaskListEntryId == newEntry.Id)));
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
