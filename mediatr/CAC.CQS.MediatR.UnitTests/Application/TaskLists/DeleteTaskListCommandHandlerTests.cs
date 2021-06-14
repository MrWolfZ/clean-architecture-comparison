using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using MediatR;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class DeleteTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<DeleteTaskListCommand> testee;

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
