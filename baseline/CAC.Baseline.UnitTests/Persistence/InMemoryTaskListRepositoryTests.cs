using CAC.Baseline.Web.Persistence;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Persistence
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
