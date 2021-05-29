using CAC.Core.Domain;

namespace CAC.CQS.Domain.TaskLists
{
    public sealed record TaskListId : EntityId<TaskList>
    {
        private TaskListId(long numericId)
            : base(numericId)
        {
        }

        public static implicit operator TaskListId(long value) => Of(value);

        public static TaskListId Of(long value) => new TaskListId(value);

        public override string ToString() => Value;
    }
}
