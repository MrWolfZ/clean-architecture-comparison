using System.Threading.Tasks;
using CAC.Core.Jobs;
using CAC.CQS.MediatR.Application.TaskLists.SendTaskListReminders;
using MediatR;

namespace CAC.CQS.MediatR.Jobs
{
    internal sealed class SendTaskListRemindersJob : IJob
    {
        private readonly IMediator mediator;

        public SendTaskListRemindersJob(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task RunAsync()
        {
            _ = await mediator.Send(new SendTaskListRemindersCommand());
        }
    }
}
