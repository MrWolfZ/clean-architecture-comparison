using System.Net;
using System.Threading.Tasks;
using CAC.Core.Application;
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
        public async Task<CreateNewTaskListCommandResponse> CreateNewTaskList(CreateNewTaskListCommand command)
        {
            return await createNewTaskListCommandHandler.ExecuteCommand(command);
        }

        [HttpPost("addTaskToList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<AddTaskToListCommandResponse> AddTaskToList(AddTaskToListCommand command)
        {
            return await addTaskToListCommandHandler.ExecuteCommand(command);
        }

        [HttpPost("markTaskAsDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(MarkTaskAsDoneCommand command)
        {
            await markTaskAsDoneCommandHandler.ExecuteCommand(command);
            return NoContent();
        }

        [HttpPost("deleteTaskList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(DeleteTaskListCommand command)
        {
            await deleteTaskListCommandHandler.ExecuteCommand(command);
            return NoContent();
        }
    }
}
