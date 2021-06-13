using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;

namespace CAC.CQS.Domain.TaskListAggregate
{
    // this is not a good name in terms of DDD, since people would usually talk about
    // just a "task" instead of a "task list item", but unfortunately the word "task"
    // already has a very specific meaning in C#
    public sealed record TaskListEntry : Entity<TaskListEntry, TaskListEntryId>
    {
        private TaskListEntry(TaskListEntryId id, string description, bool isDone)
            : base(id)
        {
            Description = description;
            IsDone = isDone;
        }

        public string Description { get; }

        public bool IsDone { get; private init; }

        public static TaskListEntry ForAddingToTaskList(TaskListId owningTaskListId, TaskListEntryId id, string description)
        {
            CheckInvariants();

            return FromRawData(id, description, false);

            void CheckInvariants()
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new DomainInvariantViolationException(owningTaskListId, "entry description must be a non-empty non-whitespace string");
                }
            }
        }

        public static TaskListEntry FromRawData(TaskListEntryId id, string description, bool isDone) => new(id, description, isDone);

        public TaskListEntry MarkAsDone() => this with { IsDone = true };
    }
}
