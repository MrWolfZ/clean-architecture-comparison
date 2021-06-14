using CAC.Core.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class DeleteTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<DeleteTaskListCommand> testee;

        public DeleteTaskListCommandHandlerTests()
        {
            testee = new DeleteTaskListCommandHandler(repository);
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}