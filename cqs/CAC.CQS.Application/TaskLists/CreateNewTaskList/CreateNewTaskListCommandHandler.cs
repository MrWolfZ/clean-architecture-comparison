using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Application.Users;
using CAC.CQS.Domain.TaskListAggregate;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Application.TaskLists.CreateNewTaskList
{
    public sealed class CreateNewTaskListCommandHandler : ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private readonly ILogger<CreateNewTaskListCommandHandler> logger;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public CreateNewTaskListCommandHandler(ITaskListRepository taskListRepository, IUserRepository userRepository, ILogger<CreateNewTaskListCommandHandler> logger)
        {
            this.taskListRepository = taskListRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        public async Task<CreateNewTaskListCommandResponse> ExecuteCommand(CreateNewTaskListCommand command)
        {
            var owner = await userRepository.GetById(command.OwnerId);

            if (owner == null)
            {
                throw new DomainEntityNotFoundException(command.OwnerId, $"user {command.OwnerId} does not exist");
            }

            var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(command.OwnerId);

            var id = await taskListRepository.GenerateId();
            var taskList = TaskList.ForOwner(owner, id, command.Name, numberOfListsOwnedByOwner);

            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{TaskListId}' for owner '{OwnerId}'...", command.Name, id, taskList.OwnerId);

            return new(taskList.Id);
        }
    }
}
