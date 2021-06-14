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

        [HttpPost("getAllTaskLists")]
        public async Task<GetAllTaskListsQueryResponse> GetAll(GetAllTaskListsQuery query, CancellationToken cancellationToken)
        {
            return await mediator.Send(query, cancellationToken);
        }

        [HttpPost("getTaskListById")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetTaskListByIdQueryResponse> GetById(GetTaskListByIdQuery query, CancellationToken cancellationToken)
        {
            return await mediator.Send(query, cancellationToken);
        }

        [HttpPost("getAllTaskListsWithPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> GetAllWithPendingEntries(GetAllTaskListsWithPendingEntriesQuery query, CancellationToken cancellationToken)
        {
            return await mediator.Send(query, cancellationToken);
        }
    }
}
