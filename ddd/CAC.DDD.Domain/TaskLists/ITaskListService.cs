using System.Threading.Tasks;

namespace CAC.DDD.Domain.TaskLists
{
    public interface ITaskListService
    {
        public Task<TaskListId> CreateNewTaskList(string name);

        public Task<bool> AddItemToTaskList(TaskListId taskListId, string description);

        public Task<bool> MarkTaskListItemAsDone(TaskListId taskListId, int itemIdx);

        public Task<TaskList?> GetTaskListById(TaskListId taskListId);
    }
}
