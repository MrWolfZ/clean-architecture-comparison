using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Basic.Infrastructure.Users;
using CAC.Core.Application;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Application.TaskLists
{
    public sealed class TaskListReminderServiceTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ITaskListReminderService testee;

        public TaskListReminderServiceTests()
        {
            testee = new TaskListReminderService(repository, new InMemoryUserRepository(), Mock.Of<ILogger<TaskListReminderService>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
