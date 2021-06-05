namespace CAC.Baseline.Web.Model
{
    // this is not a good name in terms of DDD, since people would usually talk about
    // just a "task" instead of a "task list entry", but unfortunately the word "task"
    // already has a very specific meaning in C#
    public sealed record TaskListEntry
    {
        public TaskListEntry(string description, bool isDone)
        {
            Description = description;
            IsDone = isDone;
        }

        public string Description { get; }

        public bool IsDone { get; set; }
    }
}
