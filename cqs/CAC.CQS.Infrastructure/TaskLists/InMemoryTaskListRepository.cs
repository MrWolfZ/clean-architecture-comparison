﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using CAC.Core.Infrastructure.Persistence;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;

namespace CAC.CQS.Infrastructure.TaskLists
{
    internal sealed class InMemoryTaskListRepository : InMemoryAggregateRepository<TaskList, TaskListId>, ITaskListRepository
    {
        private long entryIdCounter;

        public InMemoryTaskListRepository(IDomainEventPublisher domainEventPublisher)
            : base(domainEventPublisher)
        {
        }

        public override async Task<TaskList> Upsert(TaskList taskList, CancellationToken cancellationToken)
        {
            var all = await GetAll();
            if (all.Any(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId))
            {
                throw new UniquenessConstraintViolationException(taskList.Id, nameof(TaskList.Name), $"a task list with name '{taskList.Name}' already exists");
            }

            taskList = await base.Upsert(taskList, cancellationToken);
            all = await GetAll();

            _ = Interlocked.Exchange(ref entryIdCounter, all.SelectMany(l => l.Entries).Select(e => e.Id.NumericValue).Concat(new[] { 0L }).Max());
            return taskList;
        }

        public Task<TaskListEntryId> GenerateEntryId() => Task.FromResult(TaskListEntryId.Of(Interlocked.Increment(ref entryIdCounter)));

        public new Task<IReadOnlyCollection<TaskList>> GetAll() => base.GetAll();

        public async Task<IReadOnlyCollection<TaskList>> GetAllByOwner(UserId ownerId)
        {
            var all = await GetAll();
            return all.Where(l => l.OwnerId == ownerId).ToList();
        }

        public async Task<int> GetNumberOfTaskListsByOwner(UserId ownerId)
        {
            var all = await GetAllByOwner(ownerId);
            return all.Count;
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries()
        {
            var all = await GetAll();
            return all.Where(l => l.Entries.Any(i => !i.IsDone)).ToList();
        }

        protected override TaskListId CreateId(long numericId) => TaskListId.Of(numericId);
    }
}
