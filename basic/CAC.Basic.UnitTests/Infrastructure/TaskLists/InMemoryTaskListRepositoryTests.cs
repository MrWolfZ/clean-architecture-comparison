using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
