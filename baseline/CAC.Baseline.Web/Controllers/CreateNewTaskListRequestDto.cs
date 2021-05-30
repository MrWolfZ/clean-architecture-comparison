using System.ComponentModel.DataAnnotations;

namespace CAC.Baseline.Web.Controllers
{
    public sealed record CreateNewTaskListRequestDto
    {
        public const int MaxTaskListNameLength = 64;
        
        [Required]
        public long OwnerId { get; init; }
        
        /// <example>my task list</example>
        [Required]
        [MaxLength(MaxTaskListNameLength)]
        public string Name { get; init; } = string.Empty;
    }
}
