using System.Threading;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Core.Jobs;

namespace CAC.Basic.Jobs
{
    internal sealed class SendTaskListRemindersJob : IJob
    {
        private readonly ITaskListReminderService reminderService;

        public SendTaskListRemindersJob(ITaskListReminderService reminderService)
        {
            this.reminderService = reminderService;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await reminderService.SendTaskListReminders();
        }
    }
}
