using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Application.TaskLists
{
    public sealed class MarkTaskAsDoneCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<MarkTaskAsDoneCommand> testee;

        public MarkTaskAsDoneCommandHandlerTests()
        {
            testee = new MarkTaskAsDoneCommandHandler(repository, Mock.Of<ILogger<MarkTaskAsDoneCommandHandler>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
