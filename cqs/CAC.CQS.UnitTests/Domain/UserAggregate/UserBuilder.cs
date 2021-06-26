using System.Threading;
using CAC.CQS.Domain.UserAggregate;

namespace CAC.CQS.UnitTests.Domain.UserAggregate
{
    public sealed class UserBuilder
    {
        public static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        public static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);
        
        private static long userIdCounter = 2;

        public UserId Id { get; init; } = Interlocked.Increment(ref userIdCounter);

        public string Name { get; init; } = "premium";

        public bool IsPremium { get; init; } = true;

        public User Build() => User.FromRawData(Id, Name, IsPremium);
    }
}
