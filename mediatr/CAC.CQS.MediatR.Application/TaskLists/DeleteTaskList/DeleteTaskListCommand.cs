using System.ComponentModel.DataAnnotations;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList
{
    public sealed record DeleteTaskListCommand : IRequest
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
