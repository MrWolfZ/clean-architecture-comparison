using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using CAC.CQS.MediatR.Infrastructure.Users;
using MediatR;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class CreateNewTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse> testee;

        public CreateNewTaskListCommandHandlerTests()
        {
            testee = new CreateNewTaskListCommandHandler(repository, new InMemoryUserRepository());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
