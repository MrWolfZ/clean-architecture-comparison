using CAC.DDD.Web.Persistence;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Persistence
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        public InMemoryTaskListRepositoryTests()
        {
            Testee = new InMemoryTaskListRepository(DomainEventPublisher);
        }

        protected override ITaskListRepository Testee { get; }
    }
}
