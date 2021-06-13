using System.ComponentModel.DataAnnotations;
using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommand
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
