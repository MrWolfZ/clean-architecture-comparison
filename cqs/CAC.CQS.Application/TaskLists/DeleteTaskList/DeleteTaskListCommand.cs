using System.ComponentModel.DataAnnotations;
using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists.DeleteTaskList
{
    public sealed record DeleteTaskListCommand
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
    }
}
