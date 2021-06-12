using CAC.Core.Domain;

namespace CAC.Basic.Domain.TaskListAggregate
{
    public sealed record TaskListEntryId : EntityId<TaskListEntry>
    {
        private TaskListEntryId(long numericId)
            : base(numericId)
        {
        }

        public static implicit operator TaskListEntryId(long value) => Of(value);

        public static TaskListEntryId Of(long value) => new(value);

        public override string ToString() => Value;
    }
}
