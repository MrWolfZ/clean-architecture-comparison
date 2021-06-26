using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Infrastructure.TaskLists;
using CAC.CQS.Infrastructure.Users;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Application.TaskLists
{
    public sealed class CreateNewTaskListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<CreateNewTaskListCommand, CreateNewTaskListCommandResponse> testee;

        public CreateNewTaskListCommandHandlerTests()
        {
            testee = new CreateNewTaskListCommandHandler(repository, new InMemoryUserRepository(), Mock.Of<ILogger<CreateNewTaskListCommandHandler>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
