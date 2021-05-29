using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Domain.TaskLists.AddTaskToList
{
    public sealed class AddTaskToListCommandHandler : ICommandHandler<AddTaskToListCommand, bool>
    {
        private readonly ILogger<AddTaskToListCommandHandler> logger;
        private readonly ITaskListRepository repository;

        public AddTaskToListCommandHandler(ITaskListRepository repository, ILogger<AddTaskToListCommandHandler> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<bool> ExecuteCommand(AddTaskToListCommand command)
        {
            var taskList = await repository.GetById(command.TaskListId);

            if (taskList == null)
            {
                return false;
            }

            taskList = taskList.AddItem(command.TaskDescription);
            await repository.Upsert(taskList);

            logger.LogDebug("added task list item with description '{Description}' to task list '{TaskListName}'", command.TaskDescription, taskList.Name);

            return true;
        }
    }
}
