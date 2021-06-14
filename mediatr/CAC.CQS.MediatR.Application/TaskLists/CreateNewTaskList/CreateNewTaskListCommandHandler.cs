using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.MediatR.Application.Users;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList
{
    public sealed class CreateNewTaskListCommandHandler : IRequestHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public CreateNewTaskListCommandHandler(ITaskListRepository taskListRepository, IUserRepository userRepository)
        {
            this.taskListRepository = taskListRepository;
            this.userRepository = userRepository;
        }

        public async Task<CreateNewTaskListCommandResponse> Handle(CreateNewTaskListCommand command, CancellationToken cancellationToken)
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

            return new(taskList.Id);
        }
    }
}
