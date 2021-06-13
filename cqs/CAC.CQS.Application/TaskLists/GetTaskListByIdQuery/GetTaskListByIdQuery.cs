using System.ComponentModel.DataAnnotations;
using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed record GetTaskListByIdQuery
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
