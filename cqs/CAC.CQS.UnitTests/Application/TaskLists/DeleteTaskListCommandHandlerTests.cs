using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Application.TaskLists
{
    public sealed class DeleteTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<DeleteTaskListCommand> testee;

        public DeleteTaskListCommandHandlerTests()
        {
            testee = new DeleteTaskListCommandHandler(repository, Mock.Of<ILogger<DeleteTaskListCommandHandler>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}