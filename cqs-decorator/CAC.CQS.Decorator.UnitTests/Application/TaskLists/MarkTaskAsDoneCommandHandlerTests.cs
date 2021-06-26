using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class MarkTaskAsDoneCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<MarkTaskAsDoneCommand> testee;

        public MarkTaskAsDoneCommandHandlerTests()
        {
            testee = new MarkTaskAsDoneCommandHandler(repository);
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
