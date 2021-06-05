namespace CAC.Baseline.Web.Model
{
    // this is not a good name in terms of DDD, since people would usually talk about
    // just a "task" instead of a "task list entry", but unfortunately the word "task"
    // already has a very specific meaning in C#
    public sealed record TaskListEntry
    {
        public TaskListEntry(long id, long owningTaskListId, string description, bool isDone)
        {
            Id = id;
            OwningTaskListId = owningTaskListId;
            Description = description;
            IsDone = isDone;
        }

        public long Id { get; }
        
        public long OwningTaskListId { get; }

        public string Description { get; }

        public bool IsDone { get; set; }
    }
}
