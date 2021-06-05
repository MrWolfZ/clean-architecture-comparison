using CAC.Baseline.Web.Persistence;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Persistence
{
    [TestFixture]
    public sealed class InMemoryTaskListEntryRepositoryTests : TaskListEntryRepositoryTests
    {
        protected override ITaskListEntryRepository Testee { get; } = new InMemoryTaskListEntryRepository();
    }
}