using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone
{
    public sealed class MarkTaskAsDoneCommandHandler : IRequestHandler<MarkTaskAsDoneCommand>
    {
        private readonly ITaskListRepository taskListRepository;

        public MarkTaskAsDoneCommandHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<Unit> Handle(MarkTaskAsDoneCommand command, CancellationToken cancellationToken)
        {
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

            _ = await taskListRepository.Upsert(taskList);

            return Unit.Value;
        }
    }
}
