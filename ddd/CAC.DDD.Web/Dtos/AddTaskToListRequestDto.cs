using System.ComponentModel.DataAnnotations;

namespace CAC.DDD.Web.Dtos
{
    public sealed record AddTaskToListRequestDto
    {
        public const int MaxTaskDescriptionLength = 256;

        /// <example>my task</example>
        [Required]
        [MaxLength(MaxTaskDescriptionLength)]
        public string TaskDescription { get; init; } = string.Empty;
    }
}
