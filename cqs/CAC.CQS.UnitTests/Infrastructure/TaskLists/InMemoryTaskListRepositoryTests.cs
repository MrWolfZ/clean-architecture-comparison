using CAC.CQS.Domain.TaskLists;
using CAC.CQS.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
