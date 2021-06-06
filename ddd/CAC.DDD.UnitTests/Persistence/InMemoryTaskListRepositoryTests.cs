using CAC.DDD.Web.Persistence;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Persistence
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
