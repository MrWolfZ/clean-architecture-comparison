using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<TId, TAggregate> aggregatesById = new();
        private readonly IDomainEventPublisher domainEventPublisher;
        private long idCounter;

        protected InMemoryAggregateRepository(IDomainEventPublisher domainEventPublisher)
        {
            this.domainEventPublisher = domainEventPublisher;
        }

        public Task<TId> GenerateId() => Task.FromResult(CreateId(Interlocked.Increment(ref idCounter)));

        public virtual async Task<TAggregate> Upsert(TAggregate aggregate)
        {
            if (aggregate.IsDeleted)
            {
                _ = aggregatesById.Remove(aggregate.Id, out _);
            }
            else
            {
                _ = aggregatesById.AddOrUpdate(aggregate.Id, _ => aggregate.WithoutEvents(), (_, _) => aggregate.WithoutEvents());
                _ = Interlocked.Exchange(ref idCounter, aggregatesById.Keys.Select(id => id.NumericValue).Max());
            }

            await domainEventPublisher.Publish(aggregate.DomainEvents);
            return aggregate.WithoutEvents();
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
