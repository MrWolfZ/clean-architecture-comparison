using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.SendTaskListReminders
{
    public abstract class SendTaskListRemindersCommandTests : CommandHandlingIntegrationTestBase<SendTaskListRemindersCommand>
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);

        private long taskListEntryIdCounter;
        private long taskListIdCounter;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenTaskListToSendReminderFor_UpdatesTaskListWithDate()
        {
            var taskListCreatedAt = DateTimeOffset.UnixEpoch;
            using var d = SystemTime.WithCurrentTime(taskListCreatedAt);
            var taskList = CreateTaskList(numberOfEntries: 2);

            taskList = await TaskListRepository.Upsert(taskList);

            var now = taskListCreatedAt.Add(TaskList.ReminderDueAfter).AddDays(1);
            using var d2 = SystemTime.WithCurrentTime(now);
            await ExecuteCommand(new());

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.AreEqual(now, storedTaskList?.LastReminderSentAt);
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
