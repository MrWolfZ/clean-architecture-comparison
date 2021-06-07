using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Basic.Infrastructure.Users;
using CAC.Core.Application;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Application.TaskLists
{
    public sealed class TaskListServiceTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ITaskListService testee;

        public TaskListServiceTests()
        {
            testee = new TaskListService(repository, new InMemoryUserRepository(), Mock.Of<ILogger<TaskListService>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
