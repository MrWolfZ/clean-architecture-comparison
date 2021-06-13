﻿using System;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists
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

        public Task Handle(TaskListDomainEvent<TaskAddedToTaskListEvent> evt) => OnTaskListEdited();

        public Task Handle(TaskListDomainEvent<TaskListCreatedEvent> evt) =>
            UpdateStatistics(s => s with { NumberOfTaskListsCreated = s.NumberOfTaskListsCreated + 1 });

        public Task Handle(TaskListDomainEvent<TaskListDeletedEvent> evt) =>
            UpdateStatistics(s => s with { NumberOfTaskListsDeleted = s.NumberOfTaskListsDeleted + 1 });

        public Task Handle(TaskListDomainEvent<TaskMarkedAsDoneEvent> evt) => OnTaskListEdited();

        private Task OnTaskListEdited() => UpdateStatistics(s => s with { NumberOfTimesTaskListsWereEdited = s.NumberOfTimesTaskListsWereEdited + 1 });

        private async Task UpdateStatistics(Func<TaskListStatistics, TaskListStatistics> updateFn)
        {
            var stored = await repository.Get();
            var updated = updateFn(stored);
            await repository.Upsert(updated);
        }
    }
}