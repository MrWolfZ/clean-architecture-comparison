using CAC.Core.Domain;

namespace CAC.DDD.Web.Domain.UserAggregate
{
    public sealed record UserDomainEvent<TPayload> : DomainEvent<User, TPayload>
        where TPayload : notnull
    {
        public UserDomainEvent(User aggregate, TPayload payload)
            : base(aggregate, payload)
        {
        }
    }
}