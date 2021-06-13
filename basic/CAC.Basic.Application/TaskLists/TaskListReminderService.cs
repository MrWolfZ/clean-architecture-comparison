using System.Linq;
using System.Threading.Tasks;
using CAC.Basic.Application.Users;
using CAC.Basic.Domain.UserAggregate;
using CAC.Core.Domain;
using Microsoft.Extensions.Logging;

namespace CAC.Basic.Application.TaskLists
{
    internal sealed class TaskListReminderService : ITaskListReminderService
    {
        private readonly ILogger<TaskListReminderService> logger;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListReminderService(ITaskListRepository taskListRepository,
                                       IUserRepository userRepository,
                                       ILogger<TaskListReminderService> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
            this.userRepository = userRepository;
        }

        // in a real application you may want to use batching to deal with large
        // numbers of users and/or task lists
        public async Task SendTaskListReminders()
        {
            var premiumUsers = await userRepository.GetPremiumUsers();
            var results = await Task.WhenAll(premiumUsers.Select(SendTaskListReminderToUserIfApplicable));
            var nrOfRemindersSent = results.Count(b => b);
            logger.LogInformation("sent reminder to {NrOfUsers} users", nrOfRemindersSent);
        }

        private async Task<bool> SendTaskListReminderToUserIfApplicable(User user)
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

            _ = await Task.WhenAll(updatedTaskLists.Select(taskListRepository.Upsert));
            
            return true;
        }
    }
}
