using System;
using CAC.Basic.Domain.UserAggregate;
using CAC.Core.Domain;

namespace CAC.Basic.Domain.TaskListAggregate
{
    public sealed record TaskListDomainEvent<TPayload> : DomainEvent<TaskList, TPayload>
        where TPayload : notnull
    {
        public TaskListDomainEvent(TaskList aggregate, TPayload payload)
            : base(aggregate, payload)
        {
        }
    }

    public sealed record TaskListCreatedEvent(User Owner);

    public sealed record TaskAddedToTaskListEvent(TaskListEntry Entry);

    public sealed record TaskMarkedAsDoneEvent(TaskListEntry Entry);

    public sealed record TaskListReminderSent(DateTimeOffset ReminderSentAt);

    public sealed record TaskListDeletedEvent;
}
