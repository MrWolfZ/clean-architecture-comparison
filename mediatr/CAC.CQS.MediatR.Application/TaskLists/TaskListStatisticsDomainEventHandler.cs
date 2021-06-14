﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists
{
    internal sealed class TaskListStatisticsDomainEventHandler : INotificationHandler<TaskListDomainEvent<TaskListCreatedEvent>>,
                                                                 INotificationHandler<TaskListDomainEvent<TaskAddedToTaskListEvent>>,
                                                                 INotificationHandler<TaskListDomainEvent<TaskMarkedAsDoneEvent>>,
                                                                 INotificationHandler<TaskListDomainEvent<TaskListDeletedEvent>>
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