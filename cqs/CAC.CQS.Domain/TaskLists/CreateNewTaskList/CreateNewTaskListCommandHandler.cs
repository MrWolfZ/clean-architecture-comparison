using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.Domain.TaskLists.CreateNewTaskList
{
    public sealed class CreateNewTaskListCommandHandler : ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse>
    {
        private readonly ILogger<CreateNewTaskListCommandHandler> logger;
        private readonly ITaskListRepository repository;

        public CreateNewTaskListCommandHandler(ITaskListRepository repository, ILogger<CreateNewTaskListCommandHandler> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<CreateNewTaskListCommandResponse> ExecuteCommand(CreateNewTaskListCommand command)
        {
            var id = await repository.GenerateId();
            var taskList = TaskList.New(id, command.Name);
            await repository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{Id}'...", command.Name, id);

            return new CreateNewTaskListCommandResponse(id);
        }
    }
}
