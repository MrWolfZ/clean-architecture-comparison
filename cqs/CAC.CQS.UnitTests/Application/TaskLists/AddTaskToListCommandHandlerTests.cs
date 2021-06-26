using CAC.Core.Application;
using CAC.Core.Application.CommandHandling;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.AddTaskToList;
using CAC.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Application.TaskLists
{
    public sealed class AddTaskToListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse> testee;

        public AddTaskToListCommandHandlerTests()
        {
            testee = new AddTaskToListCommandHandler(repository, Mock.Of<ILogger<AddTaskToListCommandHandler>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}