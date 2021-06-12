using CAC.Core.Domain;

namespace CAC.Basic.Domain.UserAggregate
{
    public sealed record UserId : EntityId<User>
    {
        private UserId(long numericId)
            : base(numericId)
        {
        }

        public static implicit operator UserId(long value) => Of(value);

        public static UserId Of(long value) => new(value);

        public override string ToString() => Value;
    }
}
