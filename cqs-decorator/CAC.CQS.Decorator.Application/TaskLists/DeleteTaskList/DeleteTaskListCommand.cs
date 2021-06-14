using System.ComponentModel.DataAnnotations;
using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.DeleteTaskList
{
    public sealed record DeleteTaskListCommand
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
