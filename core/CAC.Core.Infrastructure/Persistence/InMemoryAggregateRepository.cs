﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain;

namespace CAC.Core.Infrastructure.Persistence
{
    public abstract class InMemoryAggregateRepository<TAggregate, TId> : IAggregateRepository<TAggregate, TId>
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId : EntityId<TAggregate>
    {
        private readonly ConcurrentDictionary<TId, TAggregate> aggregatesById = new ConcurrentDictionary<TId, TAggregate>();
        private long idCounter;
        
        public Task<TId> GenerateId() => Task.FromResult(CreateId(Interlocked.Increment(ref idCounter)));

        public virtual Task Upsert(TAggregate aggregate)
        {
            if (aggregate.IsDeleted)
            {
                _ = aggregatesById.Remove(aggregate.Id, out _);
                return Task.CompletedTask;
            }

            _ = aggregatesById.AddOrUpdate(aggregate.Id, _ => aggregate.WithoutEvents(), (_, _) => aggregate.WithoutEvents());
            _ = Interlocked.Exchange(ref idCounter, aggregatesById.Keys.Select(id => id.NumericValue).Max());
            return Task.CompletedTask;
        }

        public Task<TAggregate?> GetById(TId id)
        {
            var result = aggregatesById.TryGetValue(id, out var taskList) ? taskList : null;
            return Task.FromResult(result);
        }

        protected abstract TId CreateId(long numericId);

        protected Task<IReadOnlyCollection<TAggregate>> GetAll()
        {
            return Task.FromResult<IReadOnlyCollection<TAggregate>>(aggregatesById.Values.ToList());
        }
    }
}