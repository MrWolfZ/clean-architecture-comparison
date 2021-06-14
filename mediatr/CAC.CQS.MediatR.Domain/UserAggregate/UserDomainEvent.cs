using CAC.Core.Domain;
using MediatR;

namespace CAC.CQS.MediatR.Domain.UserAggregate
{
    public sealed record UserDomainEvent<TPayload> : DomainEvent<User, TPayload>, INotification
        where TPayload : notnull
    {
        public UserDomainEvent(User aggregate, TPayload payload)
            : base(aggregate, payload)
        {
        }
    }
}
