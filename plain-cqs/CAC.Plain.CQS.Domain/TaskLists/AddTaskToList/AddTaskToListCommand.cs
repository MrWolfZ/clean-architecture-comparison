using System.ComponentModel.DataAnnotations;

namespace CAC.Plain.CQS.Domain.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommand([Required] TaskListId TaskListId)
    {
        public TaskListId TaskListId { get; } = TaskListId;

        /// <example>my task</example>
        [Required]
        public string TaskDescription { get; init; } = string.Empty;
    }
}
