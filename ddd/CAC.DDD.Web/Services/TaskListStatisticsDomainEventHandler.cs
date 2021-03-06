using System;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.DDD.Web.Domain;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Persistence;

namespace CAC.DDD.Web.Services
{
    internal sealed class TaskListStatisticsDomainEventHandler : IDomainEventHandler<TaskListDomainEvent<TaskListCreatedEvent>>,
                                                                 IDomainEventHandler<TaskListDomainEvent<TaskAddedToTaskListEvent>>,
                                                                 IDomainEventHandler<TaskListDomainEvent<TaskMarkedAsDoneEvent>>,
                                                                 IDomainEventHandler<TaskListDomainEvent<TaskListDeletedEvent>>
    {
        private readonly ITaskListStatisticsRepository repository;

        public TaskListStatisticsDomainEventHandler(ITaskListStatisticsRepository repository)
        {
            this.repository = repository;
        }

        public Task Handle(TaskListDomainEvent<TaskAddedToTaskListEvent> evt, CancellationToken cancellationToken) => OnTaskListEdited();

        public Task Handle(TaskListDomainEvent<TaskListCreatedEvent> evt, CancellationToken cancellationToken) =>
            UpdateStatistics(s => s with { NumberOfTaskListsCreated = s.NumberOfTaskListsCreated + 1 });

        public Task Handle(TaskListDomainEvent<TaskListDeletedEvent> evt, CancellationToken cancellationToken) =>
            UpdateStatistics(s => s with { NumberOfTaskListsDeleted = s.NumberOfTaskListsDeleted + 1 });

        public Task Handle(TaskListDomainEvent<TaskMarkedAsDoneEvent> evt, CancellationToken cancellationToken) => OnTaskListEdited();

        private Task OnTaskListEdited() => UpdateStatistics(s => s with { NumberOfTimesTaskListsWereEdited = s.NumberOfTimesTaskListsWereEdited + 1 });

        private async Task UpdateStatistics(Func<TaskListStatistics, TaskListStatistics> updateFn)
        {
            var stored = await repository.Get();
            var updated = updateFn(stored);
            await repository.Upsert(updated);
        }
    }
}
