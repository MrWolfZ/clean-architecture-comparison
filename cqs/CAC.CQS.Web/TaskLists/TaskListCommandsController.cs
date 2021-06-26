using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Application.TaskLists.AddTaskToList;
using CAC.CQS.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Application.TaskLists.MarkTaskAsDone;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListCommandsController : ControllerBase
    {
        private readonly ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse> addTaskToListCommandHandler;
        private readonly ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse> createNewTaskListCommandHandler;
        private readonly ICommandHandler<DeleteTaskListCommand> deleteTaskListCommandHandler;
        private readonly ICommandHandler<MarkTaskAsDoneCommand> markTaskAsDoneCommandHandler;

        public TaskListCommandsController(ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse> createNewTaskListCommandHandler,
                                          ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse> addTaskToListCommandHandler,
                                          ICommandHandler<MarkTaskAsDoneCommand> markTaskAsDoneCommandHandler,
                                          ICommandHandler<DeleteTaskListCommand> deleteTaskListCommandHandler)
        {
            this.createNewTaskListCommandHandler = createNewTaskListCommandHandler;
            this.addTaskToListCommandHandler = addTaskToListCommandHandler;
            this.markTaskAsDoneCommandHandler = markTaskAsDoneCommandHandler;
            this.deleteTaskListCommandHandler = deleteTaskListCommandHandler;
        }

        [HttpPost("createNewTaskList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<CreateNewTaskListCommandResponse> CreateNewTaskList(CreateNewTaskListCommand command, CancellationToken cancellationToken)
        {
            return await createNewTaskListCommandHandler.ExecuteCommand(command, cancellationToken);
        }

        [HttpPost("addTaskToList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<AddTaskToListCommandResponse> AddTaskToList(AddTaskToListCommand command, CancellationToken cancellationToken)
        {
            return await addTaskToListCommandHandler.ExecuteCommand(command, cancellationToken);
        }

        [HttpPost("markTaskAsDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(MarkTaskAsDoneCommand command, CancellationToken cancellationToken)
        {
            await markTaskAsDoneCommandHandler.ExecuteCommand(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("deleteTaskList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(DeleteTaskListCommand command, CancellationToken cancellationToken)
        {
            await deleteTaskListCommandHandler.ExecuteCommand(command, cancellationToken);
            return NoContent();
        }
    }
}
