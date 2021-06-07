using System.Collections.Immutable;
using System.Linq;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Domain.TaskListAggregate
{
    public sealed record TaskList : AggregateRoot<TaskList, TaskListId>
    {
        private TaskList(TaskListId id, UserId ownerId, string name, ValueList<TaskListEntry> entries)
            : base(id)
        {
            OwnerId = ownerId;
            Name = name;
            Entries = entries;
        }

        public UserId OwnerId { get; }

        public string Name { get; }

        public ValueList<TaskListEntry> Entries { get; private init; }

        public static TaskList New(TaskListId id, User owner, string name, int numberOfListsOwnedByOwner)
        {
            if (!owner.IsPremium && numberOfListsOwnedByOwner > 0)
            {
                throw new DomainInvariantViolationException(id, $"non-premium user '{owner.Id}' already owns a task list");
            }

            return New(id, owner.Id, name, ValueList<TaskListEntry>.Empty).WithEvent(new TaskListCreatedEvent(owner));
        }

        public static TaskList New(TaskListId id, UserId ownerId, string name, ValueList<TaskListEntry> entries)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainInvariantViolationException(id, "name must be a non-empty non-whitespace string");
            }

            return new(id, ownerId, name, entries);
        }

        public TaskList AddEntry(TaskListEntryId id, string description)
        {
            var entry = TaskListEntry.New(Id, id, description, false);
            var updatedList = this with { Entries = Entries.Add(entry) };
            return updatedList.WithEvent(new TaskAddedToTaskListEvent(entry));
        }

        public TaskList MarkEntryAsDone(TaskListEntryId entryId)
        {
            if (Entries.All(e => e.Id != entryId))
            {
                throw new DomainInvariantViolationException(Id, $"entry '{entryId}' does not exist");
            }

            var entry = Entries.Single(e => e.Id == entryId);
            var updatedEntry = entry.MarkAsDone();
            var updatedEntries = Entries.Replace(entry, updatedEntry);
            var updatedList = this with { Entries = updatedEntries };
            return updatedList.WithEvent(new TaskMarkedAsDoneEvent(updatedEntry));
        }

        public new TaskList MarkAsDeleted() => base.MarkAsDeleted().WithEvent(new TaskListDeletedEvent());

        protected override DomainEvent<TaskList> CreateEvent<TPayload>(TPayload payload) => new TaskListDomainEvent<TPayload>(this, payload);
    }
}
