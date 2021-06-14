using System.Threading;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists
{
    internal sealed class TaskListNotificationDomainEventHandler : INotificationHandler<TaskListDomainEvent<TaskListCreatedEvent>>,
                                                                   INotificationHandler<TaskListDomainEvent<TaskAddedToTaskListEvent>>,
                                                                   INotificationHandler<TaskListDomainEvent<TaskMarkedAsDoneEvent>>,
                                                                   INotificationHandler<TaskListDomainEvent<TaskListDeletedEvent>>
    {
        private readonly IMessageQueueAdapter messageQueueAdapter;

        public TaskListNotificationDomainEventHandler(IMessageQueueAdapter messageQueueAdapter)
        {
            this.messageQueueAdapter = messageQueueAdapter;
        }

        public async Task Handle(TaskListDomainEvent<TaskAddedToTaskListEvent> evt, CancellationToken cancellationToken)
        {
            await messageQueueAdapter.Send(new TaskAddedToListMessage(evt.Aggregate.Id, evt.Payload.Entry.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskListCreatedEvent> evt, CancellationToken cancellationToken)
        {
            await messageQueueAdapter.Send(new TaskListCreatedMessage(evt.Aggregate.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskListDeletedEvent> evt, CancellationToken cancellationToken)
        {
            await messageQueueAdapter.Send(new TaskListDeletedMessage(evt.Aggregate.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskMarkedAsDoneEvent> evt, CancellationToken cancellationToken)
        {
            await messageQueueAdapter.Send(new TaskMarkedAsDoneMessage(evt.Aggregate.Id, evt.Payload.Entry.Id));
        }

        // messages

        public sealed record TaskListCreatedMessage(TaskListId TaskListId);

        public sealed record TaskAddedToListMessage(TaskListId TaskListId, TaskListEntryId TaskListEntryId);

        public sealed record TaskMarkedAsDoneMessage(TaskListId TaskListId, TaskListEntryId TaskListEntryId);

        public sealed record TaskListDeletedMessage(TaskListId TaskListId);
    }
}
