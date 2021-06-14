using System.ComponentModel.DataAnnotations;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed record GetTaskListByIdQuery : IRequest<GetTaskListByIdQueryResponse>
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
