using CAC.Core.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using CAC.CQS.Decorator.Infrastructure.Users;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class CreateNewTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse> testee;

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
