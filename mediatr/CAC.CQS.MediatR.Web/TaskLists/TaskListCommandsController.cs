using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists.AddTaskToList;
using CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList;
using CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.MediatR.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListCommandsController : ControllerBase
    {
        private readonly IMediator mediator;

        public TaskListCommandsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("createNewTaskList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<CreateNewTaskListCommandResponse> CreateNewTaskList(CreateNewTaskListCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken);
        }

        [HttpPost("addTaskToList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<AddTaskToListCommandResponse> AddTaskToList(AddTaskToListCommand command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken);
        }

        [HttpPost("markTaskAsDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(MarkTaskAsDoneCommand command, CancellationToken cancellationToken)
        {
            _ = await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("deleteTaskList")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(DeleteTaskListCommand command, CancellationToken cancellationToken)
        {
            _ = await mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
