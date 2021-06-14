using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using CAC.CQS.MediatR.Infrastructure.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class SendTaskListRemindersCommandHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<SendTaskListRemindersCommand> testee;

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
