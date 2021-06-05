using CAC.Baseline.Web.Data;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Infrastructure
{
    [TestFixture]
    public sealed class InMemoryTaskListEntryRepositoryTests : TaskListEntryRepositoryTests
    {
        protected override ITaskListEntryRepository Testee { get; } = new InMemoryTaskListEntryRepository();
    }
}