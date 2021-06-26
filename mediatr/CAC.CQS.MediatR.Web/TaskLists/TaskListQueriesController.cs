using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.MediatR.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.MediatR.Application.TaskLists.GetTaskListByIdQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.MediatR.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListQueriesController : ControllerBase
    {
        private readonly IMediator mediator;

        public TaskListQueriesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("getAllTaskLists")]
        public async Task<GetAllTaskListsQueryResponse> GetAll(CancellationToken cancellationToken)
        {
            return await mediator.Send(new GetAllTaskListsQuery(), cancellationToken);
        }

        [HttpGet("getTaskListById")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetTaskListByIdQueryResponse> GetById([FromQuery] GetTaskListByIdQuery query, CancellationToken cancellationToken)
        {
            return await mediator.Send(query, cancellationToken);
        }

        [HttpGet("getAllTaskListsWithPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> GetAllWithPendingEntries(CancellationToken cancellationToken)
        {
            return await mediator.Send(new GetAllTaskListsWithPendingEntriesQuery(), cancellationToken);
        }
    }
}
