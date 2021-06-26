using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Commands.DeleteTaskList
{
    public abstract class DeleteTaskListCommandTests : CommandHandlingIntegrationTestBase<DeleteTaskListCommand>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListId_DeletesTaskList()
        {
            var taskList = CreateTaskList(numberOfEntries: 2);

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id });

            var foundTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsNull(foundTaskList);
        }

        [Test]
        public async Task GivenNonExistingTaskListId_FailsWithEntityNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            await AssertCommandFailure(new() { TaskListId = nonExistingId }, ExpectedCommandFailure.EntityNotFound);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);
            
            await ExecuteCommand(new() { TaskListId = taskList.Id });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsDeleted);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = CreateTaskList();

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id });

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
