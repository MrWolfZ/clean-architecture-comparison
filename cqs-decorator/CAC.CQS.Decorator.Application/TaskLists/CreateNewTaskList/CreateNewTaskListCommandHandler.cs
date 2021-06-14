using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Decorator.Application.Users;
using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList
{
    public sealed class CreateNewTaskListCommandHandler : ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public CreateNewTaskListCommandHandler(ITaskListRepository taskListRepository, IUserRepository userRepository)
        {
            this.taskListRepository = taskListRepository;
            this.userRepository = userRepository;
        }

        [LogCommand]
        [ValidateCommand]
        public async Task<CreateNewTaskListCommandResponse> ExecuteCommand(CreateNewTaskListCommand command, CancellationToken cancellationToken)
        {
            var owner = await userRepository.GetById(command.OwnerId);

            if (owner == null)
            {
                throw new DomainEntityNotFoundException(command.OwnerId, $"user {command.OwnerId} does not exist");
            }

            var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(command.OwnerId);

            var id = await taskListRepository.GenerateId();
            var taskList = TaskList.ForOwner(owner, id, command.Name, numberOfListsOwnedByOwner);

            taskList = await taskListRepository.Upsert(taskList, cancellationToken);

            return new(taskList.Id);
        }
    }
}
