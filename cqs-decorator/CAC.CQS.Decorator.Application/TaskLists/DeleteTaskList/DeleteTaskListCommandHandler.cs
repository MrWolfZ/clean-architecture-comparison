using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Application.CommandHandling.Behaviors;
using CAC.Core.Domain.Exceptions;

namespace CAC.CQS.Decorator.Application.TaskLists.DeleteTaskList
{
    public sealed class DeleteTaskListCommandHandler : ICommandHandler<DeleteTaskListCommand>
    {
        private readonly ITaskListRepository taskListRepository;

        public DeleteTaskListCommandHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        [CommandLoggingBehavior]
        [CommandValidationBehavior]
        public async Task ExecuteCommand(DeleteTaskListCommand command, CancellationToken cancellationToken)
        {
            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list '{command.TaskListId}' does not exist");
            }

            taskList = taskList.MarkAsDeleted();

            _ = await taskListRepository.Upsert(taskList, cancellationToken);
        }
    }
}
