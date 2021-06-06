using System.Threading.Tasks;
using CAC.DDD.Web.Domain.TaskListAggregate;

namespace CAC.DDD.Web.Services
{
    internal sealed class TaskListNotificationService : ITaskListNotificationService
    {
        private readonly IMessageQueueAdapter messageQueueAdapter;

        public TaskListNotificationService(IMessageQueueAdapter messageQueueAdapter)
        {
            this.messageQueueAdapter = messageQueueAdapter;
        }

        public Task OnTaskListCreated(TaskList taskList) => messageQueueAdapter.Send(new TaskListCreatedMessage(taskList));

        public async Task OnTaskAddedToList(TaskList taskList, TaskListEntryId taskListEntryId)
        {
            await messageQueueAdapter.Send(new TaskAddedToListMessage(taskList!, taskListEntryId));
        }

        public async Task OnTaskMarkedAsDone(TaskList taskList, TaskListEntryId taskListEntryId)
        {
            await messageQueueAdapter.Send(new TaskMarkedAsDoneMessage(taskList!, taskListEntryId));
        }

        public Task OnTaskListDeleted(TaskListId taskListId) => messageQueueAdapter.Send(new TaskListDeletedMessage(taskListId));

        public sealed record TaskListCreatedMessage(TaskList TaskList);

        public sealed record TaskAddedToListMessage(TaskList TaskList, TaskListEntryId TaskListEntryId);

        public sealed record TaskMarkedAsDoneMessage(TaskList TaskList, TaskListEntryId TaskListEntryId);

        public sealed record TaskListDeletedMessage(TaskListId TaskListId);
    }
}
