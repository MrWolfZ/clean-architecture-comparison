using System;
using System.Collections.Generic;
using System.Linq;

namespace CAC.Baseline.Web.Model
{
    public sealed class TaskList
    {
        public const int MaxTaskListNameLength = 64;

        private readonly List<TaskListItem> items;

        public TaskList(long id, string name, IReadOnlyCollection<TaskListItem>? items = null)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id must be a positive integer", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name must be a non-empty non-whitespace string", nameof(name));
            }

            if (name.Length > MaxTaskListNameLength)
            {
                throw new ArgumentException($"task list name must not be longer than {MaxTaskListNameLength} characters, but it was {name.Length} characters long", nameof(name));
            }

            Id = id;
            Name = name;
            this.items = items?.ToList() ?? new List<TaskListItem>();
        }

        public long Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<TaskListItem> Items => items;

        public void AddItem(string description)
        {
            items.Add(new TaskListItem(description, false));
        }

        public void MarkItemAsDone(int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= Items.Count)
            {
                throw new ArgumentException($"item with index {itemIdx} does not exist", nameof(itemIdx));
            }

            items[itemIdx].MarkAsDone();
        }
    }
}
