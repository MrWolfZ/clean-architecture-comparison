using System.ComponentModel.DataAnnotations;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone
{
    public sealed record MarkTaskAsDoneCommand : IRequest
    {
        [Required]
        public TaskListId TaskListId { get; init; } = default!;
        
        [Required]
        public TaskListEntryId EntryId { get; init; } = default!;
    }
}
