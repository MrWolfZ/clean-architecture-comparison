using System.ComponentModel.DataAnnotations;
using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed record GetTaskListByIdQuery
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
