namespace CAC.Core.Domain
{
    public abstract record DomainEvent;

    public abstract record DomainEvent<TAggregate>
    {
        protected DomainEvent(TAggregate aggregate)
        {
            Aggregate = aggregate;
        }

        public TAggregate Aggregate { get; }
    }

    public abstract record DomainEvent<TAggregate, TPayload> : DomainEvent<TAggregate>
        where TAggregate : AggregateRoot
        where TPayload : notnull
    {
        protected DomainEvent(TAggregate aggregate, TPayload payload)
            : base(aggregate)
        {
            Payload = payload;
        }

        public TPayload Payload { get; }
    }
}
