using CAC.DDD.Domain.TaskLists;
using CAC.DDD.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
