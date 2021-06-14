using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.AddTaskToList
{
    public sealed class AddTaskToListCommandHandler : IRequestHandler<AddTaskToListCommand, AddTaskToListCommandResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public AddTaskToListCommandHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<AddTaskToListCommandResponse> Handle(AddTaskToListCommand command, CancellationToken cancellationToken)
        {
            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list {command.TaskListId} does not exist");
            }

            var id = await taskListRepository.GenerateEntryId();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, id, command.TaskDescription);
            taskList = taskList.AddEntry(newEntry);
            _ = await taskListRepository.Upsert(taskList);

            return new(id);
        }
    }
}
