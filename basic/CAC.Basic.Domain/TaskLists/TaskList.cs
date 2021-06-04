using System;
using System.Collections.Generic;
using System.Linq;

namespace CAC.Basic.Domain.TaskLists
{
    public sealed class TaskList
    {
        private readonly List<TaskListEntry> entries;

        public TaskList(long id, long ownerId, string name, IReadOnlyCollection<TaskListEntry>? entries = null)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id must be a positive integer", nameof(id));
            }
            
            if (ownerId <= 0)
            {
                throw new ArgumentException("owner id must be a positive integer", nameof(ownerId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name must be a non-empty non-whitespace string", nameof(name));
            }

            Id = id;
            OwnerId = ownerId;
            Name = name;
            this.entries = entries?.ToList() ?? new List<TaskListEntry>();
        }

        public long Id { get; }

        public long OwnerId { get; }

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
