using System.ComponentModel.DataAnnotations;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommand : IRequest<AddTaskToListCommandResponse>
    {
        public const int MaxTaskDescriptionLength = 256;

        [Required]
        public TaskListId TaskListId { get; init; } = default!;

        /// <example>my task</example>
        [Required]
        [MaxLength(MaxTaskDescriptionLength)]
        public string TaskDescription { get; init; } = string.Empty;
    }
}
