using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Domain.TaskListAggregate;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Application.TaskLists.AddTaskToList
{
    public sealed class AddTaskToListCommandHandler : ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse>
    {
        private readonly ILogger<AddTaskToListCommandHandler> logger;
        private readonly ITaskListRepository taskListRepository;

        public AddTaskToListCommandHandler(ITaskListRepository taskListRepository, ILogger<AddTaskToListCommandHandler> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
        }

        public async Task<AddTaskToListCommandResponse> ExecuteCommand(AddTaskToListCommand command, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);

            var taskList = await taskListRepository.GetById(command.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(command.TaskListId, $"task list {command.TaskListId} does not exist");
            }

            var id = await taskListRepository.GenerateEntryId();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, id, command.TaskDescription);
            taskList = taskList.AddEntry(newEntry);
            taskList = await taskListRepository.Upsert(taskList, cancellationToken);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", command.TaskDescription, taskList.Id);

            return new(id);
        }
    }
}
