using System.ComponentModel.DataAnnotations;

namespace CAC.Baseline.Web.Controllers
{
    public sealed record CreateNewTaskListRequestDto
    {
        /// <example>my task list</example>
        [Required]
        public string Name { get; init; } = string.Empty;
    }
}
