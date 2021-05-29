using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Domain.TaskLists.MarkTaskAsDone
{
    public sealed class MarkTaskAsDoneCommandHandler : ICommandHandler<MarkTaskAsDoneCommand, bool>
    {
        private readonly ILogger<MarkTaskAsDoneCommandHandler> logger;
        private readonly ITaskListRepository repository;

        public MarkTaskAsDoneCommandHandler(ITaskListRepository repository, ILogger<MarkTaskAsDoneCommandHandler> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<bool> ExecuteCommand(MarkTaskAsDoneCommand command)
        {
            var taskList = await repository.GetById(command.TaskListId);

            if (taskList == null)
            {
                return false;
            }

            taskList = taskList.MarkItemAsDone(command.ItemIdx);

            await repository.Upsert(taskList);

            logger.LogDebug("marked task list item '{ItemIdx}' in task list '{TaskListName}' as done", command.ItemIdx, taskList.Name);

            return true;
        }
    }
}
