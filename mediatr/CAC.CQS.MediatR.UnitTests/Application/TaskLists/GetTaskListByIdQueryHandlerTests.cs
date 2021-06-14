﻿using CAC.Core.Application;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.MediatR.Infrastructure.TaskLists;
using MediatR;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Application.TaskLists
{
    public sealed class GetTaskListByIdQueryHandlerTests
    {
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository(Mock.Of<IDomainEventPublisher>());

        private readonly IRequestHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse> testee;

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
