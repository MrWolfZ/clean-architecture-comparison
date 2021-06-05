using System.Collections.Generic;

namespace CAC.Baseline.Web.Model
{
    public sealed class TaskList
    {
        public TaskList(long id, long ownerId, string name, IList<TaskListEntry>? entries = null)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
            Entries = entries ?? new List<TaskListEntry>();
        }

        public long Id { get; }

        public long OwnerId { get; }

        public string Name { get; }

        public IList<TaskListEntry> Entries { get; }
    }
}
