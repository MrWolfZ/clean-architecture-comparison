using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Infrastructure.TaskLists;
using NUnit.Framework;

namespace CAC.Plain.CQS.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    public sealed class InMemoryTaskListRepositoryTests : TaskListRepositoryTests
    {
        protected override ITaskListRepository Testee { get; } = new InMemoryTaskListRepository();
    }
}
