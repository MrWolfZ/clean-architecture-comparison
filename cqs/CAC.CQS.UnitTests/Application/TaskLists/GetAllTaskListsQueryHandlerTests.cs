using CAC.Core.Application;
using CAC.Core.Application.QueryHandling;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Infrastructure.TaskLists;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Application.TaskLists
{
    public sealed class GetAllTaskListsQueryHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IQueryHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse> testee;

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