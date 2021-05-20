using CAC.Plain.Domain.TaskLists;
using CAC.Plain.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.Plain.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
