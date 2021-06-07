using System.Collections.Immutable;
using CAC.Core.Domain.Exceptions;

namespace CAC.Core.Domain
{
    public abstract record AggregateRoot;

    public abstract record AggregateRoot<TAggregate, TId> : AggregateRoot
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId : EntityId<TAggregate>
    {
        protected AggregateRoot(TId id)
        {
            Id = id;
        }

        public TId Id { get; }

        public bool IsDeleted { get; private init; }

        private TAggregate This => (TAggregate)this;

        public ValueList<DomainEvent<TAggregate>> DomainEvents { get; private init; } = ValueList<DomainEvent<TAggregate>>.Empty;

        protected TAggregate WithEvent<TPayload>(TPayload payload)
            where TPayload : notnull
        {
            var evt = CreateEvent(payload);
            return This with { DomainEvents = DomainEvents.Add(evt) };
        }

        protected abstract DomainEvent<TAggregate> CreateEvent<TPayload>(TPayload payload)
            where TPayload : notnull;

        protected internal TAggregate MarkAsDeleted()
        {
            if (IsDeleted)
            {
                throw new DomainInvariantViolationException(Id, $"aggregate '{Id}' is already deleted");
            }

            return This with { IsDeleted = true };
        }

        public TAggregate WithoutEvents() => This with { DomainEvents = ValueList<DomainEvent<TAggregate>>.Empty };
    }
}
