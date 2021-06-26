using CAC.Core.Application;
using CAC.Core.Application.QueryHandling;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Decorator.Infrastructure.TaskLists;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Application.TaskLists
{
    public sealed class GetTaskListByIdQueryHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IQueryHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse> testee;

        public GetTaskListByIdQueryHandlerTests()
        {
            testee = new GetTaskListByIdQueryHandler(repository);
        }

        [Test]
        public void Dummy()
        {
            Assert.IsNotNull(testee);
        }
    }
}
