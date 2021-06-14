using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList
{
    public sealed class DeleteTaskListCommandHandler : IRequestHandler<DeleteTaskListCommand>
    {
        private readonly ITaskListRepository taskListRepository;

        public DeleteTaskListCommandHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<Unit> Handle(DeleteTaskListCommand command, CancellationToken cancellationToken)
        {
            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list '{command.TaskListId}' does not exist");
            }

            taskList = taskList.MarkAsDeleted();

            _ = await taskListRepository.Upsert(taskList);

            return Unit.Value;
        }
    }
}
