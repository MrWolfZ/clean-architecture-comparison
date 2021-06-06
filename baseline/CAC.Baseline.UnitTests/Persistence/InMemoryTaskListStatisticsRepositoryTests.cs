using CAC.Baseline.Web.Persistence;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Persistence
{
    [TestFixture]
    public sealed class InMemoryTaskListStatisticsRepositoryTests : TaskListStatisticsRepositoryTests
    {
        protected override ITaskListStatisticsRepository Testee { get; } = new InMemoryTaskListStatisticsRepository();
    }
}