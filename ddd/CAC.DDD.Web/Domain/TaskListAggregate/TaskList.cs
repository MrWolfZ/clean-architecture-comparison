using System.Collections.Immutable;
using System.Linq;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Domain.TaskListAggregate
{
    public sealed record TaskList : AggregateRoot<TaskList, TaskListId>
    {
        internal const int NonPremiumUserTaskEntryCountLimit = 5;

        private TaskList(TaskListId id, UserId ownerId, bool ownerIsPremium, string name, ValueList<TaskListEntry> entries)
            : base(id)
        {
            OwnerId = ownerId;
            Name = name;
            Entries = entries;
            OwnerIsPremium = ownerIsPremium;
        }

        public UserId OwnerId { get; }

        public bool OwnerIsPremium { get; }

        public string Name { get; }

        public ValueList<TaskListEntry> Entries { get; private init; }

        public static TaskList ForOwner(User owner, TaskListId id, string name, int numberOfListsOwnedByOwner)
        {
            CheckInvariants();

            return FromRawData(id, owner.Id, owner.IsPremium, name, ValueList<TaskListEntry>.Empty).WithEvent(new TaskListCreatedEvent(owner));

            void CheckInvariants()
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new DomainInvariantViolationException(id, "name must be a non-empty non-whitespace string");
                }

                if (!owner.IsPremium && numberOfListsOwnedByOwner > 0)
                {
                    throw new DomainInvariantViolationException(id, $"non-premium user '{owner.Id}' already owns a task list");
                }
            }
        }

        public static TaskList FromRawData(TaskListId id, UserId ownerId, bool ownerIsPremium, string name, ValueList<TaskListEntry> entries) => new(id, ownerId, ownerIsPremium, name, entries);

        public TaskList AddEntry(TaskListEntry entry)
        {
            CheckInvariants();

            var updatedList = this with { Entries = Entries.Add(entry) };
            return updatedList.WithEvent(new TaskAddedToTaskListEvent(entry));

            void CheckInvariants()
            {
                if (!OwnerIsPremium && Entries.Count >= NonPremiumUserTaskEntryCountLimit)
                {
                    throw new DomainInvariantViolationException(Id, $"non-premium user {OwnerId} can only have at most {NonPremiumUserTaskEntryCountLimit} tasks in their list");
                }
            }
        }

        public TaskList MarkEntryAsDone(TaskListEntryId entryId)
        {
            CheckInvariants();

            var entry = Entries.Single(e => e.Id == entryId);
            var updatedEntry = entry.MarkAsDone();
            var updatedEntries = Entries.Replace(entry, updatedEntry);
            var updatedList = this with { Entries = updatedEntries };
            return updatedList.WithEvent(new TaskMarkedAsDoneEvent(updatedEntry));

            void CheckInvariants()
            {
                if (Entries.All(e => e.Id != entryId))
                {
                    throw new DomainInvariantViolationException(Id, $"entry '{entryId}' does not exist");
                }
            }
        }

        public new TaskList MarkAsDeleted() => base.MarkAsDeleted().WithEvent(new TaskListDeletedEvent());

        protected override DomainEvent<TaskList> CreateEvent<TPayload>(TPayload payload) => new TaskListDomainEvent<TPayload>(this, payload);
    }
}
