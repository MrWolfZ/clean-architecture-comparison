using System.Net;
using System.Threading.Tasks;
using CAC.CQS.Domain.TaskLists;
using CAC.CQS.Domain.TaskLists.AddTaskToList;
using CAC.CQS.Domain.TaskLists.CreateNewTaskList;
using CAC.CQS.Domain.TaskLists.GetTaskListById;
using CAC.CQS.Domain.TaskLists.MarkTaskAsDone;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListsController : ControllerBase
    {
        private readonly AddTaskToListCommandHandler addTaskToListCommandHandler;
        private readonly CreateNewTaskListCommandHandler createNewTaskListCommandHandler;
        private readonly GetTaskListByIdQueryHandler getTaskListByIdQueryHandler;
        private readonly MarkTaskAsDoneCommandHandler markTaskAsDoneCommandHandler;

        public TaskListsController(
            CreateNewTaskListCommandHandler createNewTaskListCommandHandler,
            AddTaskToListCommandHandler addTaskToListCommandHandler,
            MarkTaskAsDoneCommandHandler markTaskAsDoneCommandHandler,
            GetTaskListByIdQueryHandler getTaskListByIdQueryHandler)
        {
            this.createNewTaskListCommandHandler = createNewTaskListCommandHandler;
            this.addTaskToListCommandHandler = addTaskToListCommandHandler;
            this.markTaskAsDoneCommandHandler = markTaskAsDoneCommandHandler;
            this.getTaskListByIdQueryHandler = getTaskListByIdQueryHandler;
        }

        [HttpPost("createNewTaskList")]
        public async Task<ActionResult<CreateNewTaskListCommandResponse>> CreateNewTaskList(CreateNewTaskListCommand command)
        {
            return Ok(await createNewTaskListCommandHandler.ExecuteCommand(command));
        }

        [HttpPost("addTaskToList")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddTaskToList(AddTaskToListCommand command)
        {
            var wasFound = await addTaskToListCommandHandler.ExecuteCommand(command);
            return wasFound ? NoContent() : NotFound();
        }

        [HttpPost("markTaskAsDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(MarkTaskAsDoneCommand command)
        {
            var wasFound = await markTaskAsDoneCommandHandler.ExecuteCommand(command);
            return wasFound ? NoContent() : NotFound();
        }

        [HttpGet("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GetTaskListByIdQueryResponse>> GetTaskListById(TaskListId taskListId)
        {
            var response = await getTaskListByIdQueryHandler.ExecuteQuery(taskListId);
            return response == null ? NotFound() : Ok(response);
        }
    }
}
