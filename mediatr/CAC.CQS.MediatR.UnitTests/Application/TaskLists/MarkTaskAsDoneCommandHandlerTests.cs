using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using MediatR;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class MarkTaskAsDoneCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<MarkTaskAsDoneCommand> testee;

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
