using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Infrastructure.TaskLists
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