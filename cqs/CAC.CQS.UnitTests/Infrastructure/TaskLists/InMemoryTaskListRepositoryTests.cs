using CAC.CQS.Application.TaskLists;
using CAC.CQS.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Infrastructure.TaskLists
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