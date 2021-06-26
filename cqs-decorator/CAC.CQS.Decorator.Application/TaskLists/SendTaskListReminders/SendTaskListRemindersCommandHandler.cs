using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Application.CommandHandling.Behaviors;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.Users;
using CAC.CQS.Decorator.Domain.UserAggregate;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Decorator.Application.TaskLists.SendTaskListReminders
{
    public sealed class SendTaskListRemindersCommandHandler : ICommandHandler<SendTaskListRemindersCommand>
    {
        private readonly ILogger<SendTaskListRemindersCommandHandler> logger;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public SendTaskListRemindersCommandHandler(ITaskListRepository taskListRepository, IUserRepository userRepository, ILogger<SendTaskListRemindersCommandHandler> logger)
        {
            this.taskListRepository = taskListRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        [CommandLoggingBehavior(LogException = false)]
        [CommandValidationBehavior]
        public async Task ExecuteCommand(SendTaskListRemindersCommand command, CancellationToken cancellationToken)
        {
            var premiumUsers = await userRepository.GetPremiumUsers();
            var results = await Task.WhenAll(premiumUsers.Select(u => SendTaskListReminderToUserIfApplicable(u, cancellationToken)));
            var nrOfRemindersSent = results.Count(b => b);
            logger.LogInformation("sent reminder to {NrOfUsers} users", nrOfRemindersSent);
        }

        private async Task<bool> SendTaskListReminderToUserIfApplicable(User user, CancellationToken cancellationToken)
        {
            if (!user.IsEligibleForReminders())
            {
                return false;
            }

            var taskLists = await taskListRepository.GetAllByOwner(user.Id);

            var listsDueForReminder = taskLists.Where(tl => tl.IsDueForReminder()).ToList();

            if (!listsDueForReminder.Any())
            {
                return false;
            }

            logger.LogInformation("sending reminder for {NrOfTaskLists} task lists to user '{UserId}'...", listsDueForReminder.Count, user.Id);

            // in a real application we would send a reminder through some channel here, e.g. via e-mail

            var lastReminderSentAt = SystemTime.Now;

            var updatedTaskLists = listsDueForReminder.Select(tl => tl.WithReminderSentAt(lastReminderSentAt));

            _ = await Task.WhenAll(updatedTaskLists.Select(tl => taskListRepository.Upsert(tl, cancellationToken)));

            return true;
        }
    }
}
