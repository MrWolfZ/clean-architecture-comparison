using System.Collections.Immutable;
using CAC.Core.Domain;

namespace CAC.Plain.Domain.TaskLists
{
    public sealed record TaskList : Entity<TaskList, TaskListId>
    {
        public const int MaxTaskListNameLength = 64;

        private TaskList(TaskListId id, string name)
            : base(id)
        {
            Name = name;
        }

        public string Name { get; }

        public ValueList<TaskListItem> Items { get; private init; } = ValueList<TaskListItem>.Empty;

        public static TaskList New(TaskListId id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException(id, "name must be a non-empty non-whitespace string");
            }

            if (name.Length > MaxTaskListNameLength)
            {
                throw new DomainValidationException(id, $"task list name must not be longer than {MaxTaskListNameLength} characters, but it was {name.Length} characters long");
            }

            return new TaskList(id, name);
        }

        public TaskList AddItem(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new DomainValidationException(Id, "item description must be a non-empty non-whitespace string");
            }

            return this with
            {
                Items = Items.Add(TaskListItem.New(description, false)),
            };
        }

        public TaskList MarkItemAsDone(int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= Items.Count)
            {
                throw new DomainValidationException(Id, $"item with index {itemIdx} does not exist");
            }

            return this with
            {
                Items = Items.SetItem(itemIdx, Items[itemIdx].MarkAsDone()),
            };
        }
    }
}
