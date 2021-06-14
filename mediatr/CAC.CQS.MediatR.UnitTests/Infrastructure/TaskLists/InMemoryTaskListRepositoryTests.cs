using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Infrastructure.TaskLists
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