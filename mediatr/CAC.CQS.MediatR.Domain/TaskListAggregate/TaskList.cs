using System;
using System.Collections.Immutable;
using System.Linq;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.MediatR.Domain.UserAggregate;

namespace CAC.CQS.MediatR.Domain.TaskListAggregate
{
    public sealed record TaskList : AggregateRoot<TaskList, TaskListId>
    {
        internal const int NonPremiumUserTaskEntryCountLimit = 5;
        internal static readonly TimeSpan ReminderDueAfter = TimeSpan.FromDays(7);

        private TaskList(TaskListId id, UserId ownerId, bool ownerIsPremium, string name, ValueList<TaskListEntry> entries, DateTimeOffset lastChangedAt, DateTimeOffset? lastReminderSentAt)
            : base(id)
        {
            OwnerId = ownerId;
            OwnerIsPremium = ownerIsPremium;
            Name = name;
            Entries = entries;
            LastChangedAt = lastChangedAt;
            LastReminderSentAt = lastReminderSentAt;
        }

        public UserId OwnerId { get; }

        public bool OwnerIsPremium { get; }

        public string Name { get; }

        public ValueList<TaskListEntry> Entries { get; private init; }

        public DateTimeOffset LastChangedAt { get; private init; }

        public DateTimeOffset? LastReminderSentAt { get; private init; }

        public static TaskList ForOwner(User owner, TaskListId id, string name, int numberOfListsOwnedByOwner)
        {
            CheckInvariants();

            return FromRawData(id, owner.Id, owner.IsPremium, name, ValueList<TaskListEntry>.Empty, SystemTime.Now, null).WithEvent(new TaskListCreatedEvent(owner));

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

        public static TaskList FromRawData(TaskListId id,
                                           UserId ownerId,
                                           bool ownerIsPremium,
                                           string name,
                                           ValueList<TaskListEntry> entries,
                                           DateTimeOffset lastChangedAt,
                                           DateTimeOffset? lastReminderSentAt)
            => new(id, ownerId, ownerIsPremium, name, entries, lastChangedAt, lastReminderSentAt);

        public TaskList AddEntry(TaskListEntry entry)
        {
            CheckInvariants();

            var updatedList = this with { Entries = Entries.Add(entry), LastChangedAt = SystemTime.Now };
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
            var updatedList = this with { Entries = updatedEntries, LastChangedAt = SystemTime.Now };
            return updatedList.WithEvent(new TaskMarkedAsDoneEvent(updatedEntry));

            void CheckInvariants()
            {
                if (Entries.All(e => e.Id != entryId))
                {
                    throw new DomainInvariantViolationException(Id, $"entry '{entryId}' does not exist");
                }
            }
        }

        public new TaskList MarkAsDeleted()
        {
            var updatedList = base.MarkAsDeleted() with { LastChangedAt = SystemTime.Now };
            return updatedList.WithEvent(new TaskListDeletedEvent());
        }

        public TaskList WithReminderSentAt(DateTimeOffset reminderSentAt)
        {
            var updatedList = this with { LastReminderSentAt = reminderSentAt, LastChangedAt = reminderSentAt };
            return updatedList.WithEvent(new TaskListReminderSent(reminderSentAt));
        }

        public bool IsDueForReminder() => LastChangedAtAboveReminderThreshold() && LastNotificationSentAtAboveReminderThreshold() && HasPendingEntry();

        private bool LastChangedAtAboveReminderThreshold() => SystemTime.Now - LastChangedAt > ReminderDueAfter;

        private bool LastNotificationSentAtAboveReminderThreshold() => LastReminderSentAt == null || SystemTime.Now - LastReminderSentAt > ReminderDueAfter;

        private bool HasPendingEntry() => Entries.Any(e => !e.IsDone);

        protected override DomainEvent<TaskList> CreateEvent<TPayload>(TPayload payload) => new TaskListDomainEvent<TPayload>(this, payload);
    }
}
