using System.ComponentModel.DataAnnotations;
using CAC.Basic.Domain.UserAggregate;

namespace CAC.Basic.Web.TaskLists
{
    public sealed record CreateNewTaskListRequestDto
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
