namespace CAC.DDD.Domain.TaskLists
{
    // this is not a good name in terms of DDD, since people would usually talk about
    // just a "task" instead of a "task list item", but unfortunately the word "task"
    // already has a very specific meaning in C#
    public record TaskListItem
    {
        private TaskListItem(string description, bool isDone)
        {
            Description = description;
            IsDone = isDone;
        }

        public string Description { get; }

        public bool IsDone { get; init; }

        public static TaskListItem New(string description, bool isDone) => new TaskListItem(description, isDone);

        public TaskListItem MarkAsDone() => this with { IsDone = true };
    }
}
