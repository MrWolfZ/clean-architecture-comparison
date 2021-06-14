using CAC.Core.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.AddTaskToList;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class AddTaskToListCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<AddTaskToListCommand, AddTaskToListCommandResponse> testee;

        public AddTaskToListCommandHandlerTests()
        {
            testee = new AddTaskToListCommandHandler(repository);
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}