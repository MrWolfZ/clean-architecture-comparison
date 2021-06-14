using System.ComponentModel.DataAnnotations;
using CAC.CQS.Decorator.Domain.UserAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommand
    {
        public const int MaxTaskListNameLength = 64;

        [Required]
        public UserId OwnerId { get; init; } = default!;

        /// <example>my task list</example>
        [Required]
        [MaxLength(MaxTaskListNameLength)]
        public string Name { get; init; } = string.Empty;
    }
}
