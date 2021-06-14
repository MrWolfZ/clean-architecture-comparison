using System.ComponentModel.DataAnnotations;
using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone
{
    public sealed record MarkTaskAsDoneCommand
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
        
        [Required]
        public TaskListEntryId EntryId { get; init; } = default!;
    }
}
