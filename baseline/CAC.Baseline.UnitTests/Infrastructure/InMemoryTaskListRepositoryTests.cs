using CAC.Baseline.Web.Data;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Infrastructure
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
