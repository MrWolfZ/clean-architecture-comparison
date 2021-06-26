using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Application.TaskLists.MarkTaskAsDone
{
    public sealed class MarkTaskAsDoneCommandHandler : ICommandHandler<MarkTaskAsDoneCommand>
    {
        private readonly ILogger<MarkTaskAsDoneCommandHandler> logger;
        private readonly ITaskListRepository taskListRepository;

        public MarkTaskAsDoneCommandHandler(ITaskListRepository taskListRepository, ILogger<MarkTaskAsDoneCommandHandler> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
        }

        public async Task ExecuteCommand(MarkTaskAsDoneCommand command, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);

            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list '{command.TaskListId}' does not exist");
            }

            if (taskList.Entries.All(e => e.Id != command.EntryId))
            {
                throw new DomainEntityNotFoundException(command.EntryId, $"entry '{command.EntryId}' does not exist");
            }

            taskList = taskList.MarkEntryAsDone(command.EntryId);

            taskList = await taskListRepository.Upsert(taskList, cancellationToken);

            logger.LogDebug("marked task list entry '{EntryId}' in task list '{TaskListName}' as done", command.EntryId, taskList.Name);
        }
    }
}
