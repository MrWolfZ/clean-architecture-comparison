using System.Threading;
using CAC.DDD.Web.Domain.TaskListAggregate;

namespace CAC.DDD.UnitTests.Domain.TaskListAggregate
{
    public sealed record TaskListEntryBuilder
    {
        private static long taskListEntryIdCounter;

        public TaskListEntryBuilder()
        {
            Description = $"task {Id.NumericValue}";
        }

        public TaskListEntryId Id { get; init; } = Interlocked.Increment(ref taskListEntryIdCounter);

        public string Description { get; init; }
        
        public bool IsDone { get; init; }

        public TaskListEntry Build() => TaskListEntry.FromRawData(Id, Description, IsDone);
    }
}
