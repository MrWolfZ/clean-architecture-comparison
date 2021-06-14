using CAC.Core.Application;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using CAC.CQS.Decorator.Infrastructure.Users;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class SendTaskListRemindersCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly ICommandHandler<SendTaskListRemindersCommand> testee;

        public SendTaskListRemindersCommandHandlerTests()
        {
            testee = new SendTaskListRemindersCommandHandler(repository, new InMemoryUserRepository(), Mock.Of<ILogger<SendTaskListRemindersCommandHandler>>());
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
