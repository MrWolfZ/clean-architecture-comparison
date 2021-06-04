namespace CAC.Basic.Domain.TaskLists
{
    // to keep this example simple, we're just gathering some trivial statistics; in
    // a real application this will likely be much more involved
    public sealed record TaskListStatistics
    {
        public long NumberOfTaskListsCreated { get; set; }

        public long NumberOfTimesTaskListsWereEdited { get; set; }

        public long NumberOfTaskListsDeleted { get; set; }
    }
}
