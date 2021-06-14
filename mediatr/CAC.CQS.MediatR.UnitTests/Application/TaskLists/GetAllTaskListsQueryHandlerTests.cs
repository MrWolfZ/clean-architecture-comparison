using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using MediatR;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class GetAllTaskListsQueryHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse> testee;

        public GetAllTaskListsQueryHandlerTests()
        {
            testee = new GetAllTaskListsQueryHandler(repository);
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
