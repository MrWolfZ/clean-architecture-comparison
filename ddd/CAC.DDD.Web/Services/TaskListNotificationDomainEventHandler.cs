using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.DDD.Web.Domain.TaskListAggregate;

namespace CAC.DDD.Web.Services
{
    internal sealed class TaskListNotificationDomainEventHandler : IDomainEventHandler<TaskListDomainEvent<TaskListCreatedEvent>>,
                                                                   IDomainEventHandler<TaskListDomainEvent<TaskAddedToTaskListEvent>>,
                                                                   IDomainEventHandler<TaskListDomainEvent<TaskMarkedAsDoneEvent>>,
                                                                   IDomainEventHandler<TaskListDomainEvent<TaskListDeletedEvent>>
    {
        private readonly IMessageQueueAdapter messageQueueAdapter;

        public TaskListNotificationDomainEventHandler(IMessageQueueAdapter messageQueueAdapter)
        {
            this.messageQueueAdapter = messageQueueAdapter;
        }

        public async Task Handle(TaskListDomainEvent<TaskAddedToTaskListEvent> evt)
        {
            await messageQueueAdapter.Send(new TaskAddedToListMessage(evt.Aggregate.Id, evt.Payload.Entry.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskListCreatedEvent> evt)
        {
            await messageQueueAdapter.Send(new TaskListCreatedMessage(evt.Aggregate.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskListDeletedEvent> evt)
        {
            await messageQueueAdapter.Send(new TaskListDeletedMessage(evt.Aggregate.Id));
        }

        public async Task Handle(TaskListDomainEvent<TaskMarkedAsDoneEvent> evt)
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
