using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Persistence;

namespace CAC.Baseline.Web.Services
{
    internal sealed class TaskListNotificationService : ITaskListNotificationService
    {
        private readonly IMessageQueueAdapter messageQueueAdapter;
        private readonly ITaskListRepository taskListRepository;

        public TaskListNotificationService(IMessageQueueAdapter messageQueueAdapter, ITaskListRepository taskListRepository)
        {
            this.messageQueueAdapter = messageQueueAdapter;
            this.taskListRepository = taskListRepository;
        }

        public Task OnTaskListCreated(TaskList taskList) => messageQueueAdapter.Send(new TaskListCreatedMessage(taskList));

        public async Task OnTaskAddedToList(TaskListEntry taskListEntry)
        {
            var taskList = await taskListRepository.GetById(taskListEntry.OwningTaskListId);
            await messageQueueAdapter.Send(new TaskAddedToListMessage(taskList!, taskListEntry.Id));
        }

        public async Task OnTaskMarkedAsDone(long taskListId, long taskListEntryId)
        {
            var taskList = await taskListRepository.GetById(taskListId);
            await messageQueueAdapter.Send(new TaskMarkedAsDoneMessage(taskList!, taskListEntryId));
        }

        public Task OnTaskListDeleted(long taskListId) => messageQueueAdapter.Send(new TaskListDeletedMessage(taskListId));

        public sealed record TaskListCreatedMessage(TaskList TaskList);

        public sealed record TaskAddedToListMessage(TaskList TaskList, long TaskListEntryId);

        public sealed record TaskMarkedAsDoneMessage(TaskList TaskList, long TaskListEntryId);

        public sealed record TaskListDeletedMessage(long TaskListId);
    }
}
