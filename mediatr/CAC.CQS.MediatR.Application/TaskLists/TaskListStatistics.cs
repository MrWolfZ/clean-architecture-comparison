namespace CAC.CQS.MediatR.Application.TaskLists
{
    // to keep this example simple, we're just gathering some trivial statistics; in
    // a real application this will likely be much more involved
    public sealed record TaskListStatistics
    {
        public long NumberOfTaskListsCreated { get; init; }

        public long NumberOfTimesTaskListsWereEdited { get; init; }

        public long NumberOfTaskListsDeleted { get; init; }
    }
}