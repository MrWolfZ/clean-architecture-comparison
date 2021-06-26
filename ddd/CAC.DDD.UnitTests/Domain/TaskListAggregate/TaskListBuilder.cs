using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CAC.DDD.UnitTests.Domain.UserAggregate;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.UnitTests.Domain.TaskListAggregate
{
    public sealed record TaskListBuilder
    {
        private static long taskListIdCounter;

        public TaskListBuilder()
        {
            Name = $"list {Id.NumericValue}";
        }

        public TaskListId Id { get; init; } = Interlocked.Increment(ref taskListIdCounter);

        public User Owner { get; init; } = UserBuilder.PremiumOwner;

        public string Name { get; init; }

        public ValueList<TaskListEntry> Entries { get; init; } = ValueList<TaskListEntry>.Empty;

        public TaskListBuilder WithName(string name) => this with { Name = name };

        public TaskListBuilder WithOwner(User user) => this with { Owner = user };

        public TaskListBuilder WithNonPremiumOwner() => this with { Owner = UserBuilder.NonPremiumOwner };

        public TaskListBuilder WithPendingEntries(int numberOfEntries)
        {
            var entries = Enumerable.Range(1, numberOfEntries).Select(_ => new TaskListEntryBuilder().Build()).ToValueList();
            return this with { Entries = entries };
        }

        public TaskList Build() => TaskList.FromRawData(Id, Owner.Id, Owner.IsPremium, Name, Entries);
    }
}
