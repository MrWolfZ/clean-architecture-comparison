using System;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.UnitTests.Domain.TaskListAggregate;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Commands.SendTaskListReminders
{
    public abstract class SendTaskListRemindersCommandTests : CommandHandlingIntegrationTestBase<SendTaskListRemindersCommand>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task GivenTaskListToSendReminderFor_UpdatesTaskListWithDate()
        {
            var taskListCreatedAt = DateTimeOffset.UnixEpoch;
            using var d = SystemTime.WithCurrentTime(taskListCreatedAt);
            var taskList = new TaskListBuilder().WithPendingEntries(2).Build();

            taskList = await TaskListRepository.Upsert(taskList);

            var now = taskListCreatedAt.Add(TaskList.ReminderDueAfter).AddDays(1);
            using var d2 = SystemTime.WithCurrentTime(now);
            await ExecuteCommand(new());

            var storedTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.AreEqual(now, storedTaskList?.LastReminderSentAt);
        }
    }
}
