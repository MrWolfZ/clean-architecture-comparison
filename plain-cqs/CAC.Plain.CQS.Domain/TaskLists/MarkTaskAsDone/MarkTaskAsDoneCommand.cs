using System.ComponentModel.DataAnnotations;

namespace CAC.Plain.CQS.Domain.TaskLists.MarkTaskAsDone
{
    public sealed record MarkTaskAsDoneCommand([Required] TaskListId TaskListId)
    {
        public TaskListId TaskListId { get; } = TaskListId;

        [Required]
        public int ItemIdx { get; init; }
    }
}
