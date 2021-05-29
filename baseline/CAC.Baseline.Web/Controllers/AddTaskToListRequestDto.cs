using System.ComponentModel.DataAnnotations;

namespace CAC.Baseline.Web.Controllers
{
    public sealed record AddTaskToListRequestDto
    {
        /// <example>my task</example>
        [Required]
        public string TaskDescription { get; init; } = string.Empty;
    }
}
