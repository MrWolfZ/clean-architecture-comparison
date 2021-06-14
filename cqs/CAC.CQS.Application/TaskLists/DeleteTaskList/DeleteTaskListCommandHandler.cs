using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Application.TaskLists.DeleteTaskList
{
    public sealed class DeleteTaskListCommandHandler : ICommandHandler<DeleteTaskListCommand>
    {
        private readonly ILogger<DeleteTaskListCommandHandler> logger;
        private readonly ITaskListRepository taskListRepository;

        public DeleteTaskListCommandHandler(ITaskListRepository taskListRepository, ILogger<DeleteTaskListCommandHandler> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
        }

        public async Task ExecuteCommand(DeleteTaskListCommand command, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);

            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list '{command.TaskListId}' does not exist");
            }

            taskList = taskList.MarkAsDeleted();

            _ = await taskListRepository.Upsert(taskList, cancellationToken);

            logger.LogDebug("deleted task list '{TaskListId}'", command.TaskListId);
        }
    }
}
