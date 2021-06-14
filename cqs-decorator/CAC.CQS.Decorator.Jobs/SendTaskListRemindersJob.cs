using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Jobs;
using CAC.CQS.Decorator.Application.TaskLists.SendTaskListReminders;

namespace CAC.CQS.Decorator.Jobs
{
    internal sealed class SendTaskListRemindersJob : IJob
    {
        private readonly ICommandHandler<SendTaskListRemindersCommand> sendTaskListRemindersCommandHandler;

        public SendTaskListRemindersJob(ICommandHandler<SendTaskListRemindersCommand> sendTaskListRemindersCommandHandler)
        {
            this.sendTaskListRemindersCommandHandler = sendTaskListRemindersCommandHandler;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await sendTaskListRemindersCommandHandler.ExecuteCommand(new(), cancellationToken);
        }
    }
}
