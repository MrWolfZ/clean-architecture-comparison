using System;
using System.Collections.Generic;
using System.Linq;

namespace CAC.Baseline.Web.Model
{
    public sealed class TaskList
    {
        public const int MaxTaskListNameLength = 64;

        private readonly List<TaskListEntry> entries;

        public TaskList(long id, string name, IReadOnlyCollection<TaskListEntry>? entries = null)
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
            this.entries = entries?.ToList() ?? new List<TaskListEntry>();
        }

        public long Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<TaskListEntry> Entries => entries;

        public void AddEntry(string description)
        {
            entries.Add(new TaskListEntry(description, false));
        }

        public void MarkEntryAsDone(int entryIdx)
        {
            if (entryIdx < 0 || entryIdx >= Entries.Count)
            {
                throw new ArgumentException($"entry with index {entryIdx} does not exist", nameof(entryIdx));
            }

            entries[entryIdx].MarkAsDone();
        }
    }
}
