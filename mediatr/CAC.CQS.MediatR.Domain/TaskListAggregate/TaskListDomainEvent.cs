using System;
using CAC.Core.Domain;
using CAC.CQS.MediatR.Domain.UserAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Domain.TaskListAggregate
{
    public sealed record TaskListDomainEvent<TPayload> : DomainEvent<TaskList, TPayload>, INotification
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
