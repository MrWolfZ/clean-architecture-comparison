using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CAC.Plain.Domain.TaskLists
{
    internal sealed class TaskListService : ITaskListService
    {
        private readonly ILogger<TaskListService> logger;
        private readonly ITaskListRepository repository;

        public TaskListService(ITaskListRepository repository, ILogger<TaskListService> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<TaskListId> CreateNewTaskList(string name)
        {
            var id = await repository.GenerateId();
            var taskList = TaskList.New(id, name);
            await repository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{Id}'...", name, id);

            return id;
        }

        public async Task<bool> AddItemToTaskList(TaskListId taskListId, string description)
        {
            var taskList = await GetTaskListById(taskListId);

            if (taskList == null)
            {
                return false;
            }

            taskList = taskList.AddItem(description);
            await repository.Upsert(taskList);

            logger.LogDebug("added task list item with description '{Description}' to task list '{TaskListName}'", description, taskList.Name);

            return true;
        }

        public Task<TaskList?> GetTaskListById(TaskListId taskListId) => repository.GetById(taskListId);

        public async Task<bool> MarkTaskListItemAsDone(TaskListId taskListId, int itemIdx)
        {
            var taskList = await GetTaskListById(taskListId);

            if (taskList == null)
            {
                return false;
            }

            taskList = taskList.MarkItemAsDone(itemIdx);

            await repository.Upsert(taskList);

            logger.LogDebug("marked task list item '{ItemIdx}' in task list '{TaskListName}' as done", itemIdx, taskList.Name);

            return true;
        }
    }
}
