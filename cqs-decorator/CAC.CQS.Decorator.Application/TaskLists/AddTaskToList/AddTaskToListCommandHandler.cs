using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Application.CommandHandling.Behaviors;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.AddTaskToList
{
    public sealed class AddTaskToListCommandHandler : ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public AddTaskToListCommandHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        [CommandLoggingBehavior]
        [CommandValidationBehavior]
        public async Task<AddTaskToListCommandResponse> ExecuteCommand(AddTaskToListCommand command, CancellationToken cancellationToken)
        {
            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list {command.TaskListId} does not exist");
            }

            var id = await taskListRepository.GenerateEntryId();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, id, command.TaskDescription);
            taskList = taskList.AddEntry(newEntry);
            taskList = await taskListRepository.Upsert(taskList, cancellationToken);

            return new(id);
        }
    }
}
