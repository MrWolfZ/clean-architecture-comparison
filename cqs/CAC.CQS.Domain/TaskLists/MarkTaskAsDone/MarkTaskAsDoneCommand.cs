using System.ComponentModel.DataAnnotations;

namespace CAC.CQS.Domain.TaskLists.MarkTaskAsDone
{
    public sealed record MarkTaskAsDoneCommand([Required] TaskListId TaskListId)
    {
        public TaskListId TaskListId { get; } = TaskListId;

        [Required]
        public int ItemIdx { get; init; }
    }
}
