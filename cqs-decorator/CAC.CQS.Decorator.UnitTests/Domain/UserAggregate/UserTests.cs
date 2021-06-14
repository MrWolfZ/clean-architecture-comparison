using CAC.CQS.Decorator.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Domain.UserAggregate
{
    public sealed class UserTests
    {
        [Test]
        public void IsEligibleForReminders_GivenUserIsNonPremium_ReturnsFalse()
        {
            var user = User.FromRawData(1, "non-premium", false);
            Assert.IsFalse(user.IsEligibleForReminders());
        }

        [Test]
        public void IsEligibleForReminders_GivenUserIsPremium_ReturnsTrue()
        {
            var user = User.FromRawData(1, "premium", true);
            Assert.IsTrue(user.IsEligibleForReminders());
        }
    }
}