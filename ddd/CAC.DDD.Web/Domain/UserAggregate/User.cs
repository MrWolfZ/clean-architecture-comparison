using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;

namespace CAC.DDD.Web.Domain.UserAggregate
{
    public sealed record User : AggregateRoot<User, UserId>
    {
        private User(UserId id, string name, bool isPremium)
            : base(id)
        {
            Name = name;
            IsPremium = isPremium;
        }

        public string Name { get; }

        public bool IsPremium { get; }

        public static User New(UserId id, string name, bool isPremium)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainInvariantViolationException(id, "name must be a non-empty non-whitespace string");
            }

            return new User(id, name, isPremium);
        }

        protected override DomainEvent<User> CreateEvent<TPayload>(TPayload payload) => new UserDomainEvent<TPayload>(this, payload);
    }
}
