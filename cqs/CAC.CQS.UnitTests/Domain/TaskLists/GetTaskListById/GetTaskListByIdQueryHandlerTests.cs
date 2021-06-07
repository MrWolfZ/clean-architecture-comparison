using System.Threading.Tasks;
using CAC.CQS.Domain.TaskLists;
using CAC.CQS.Domain.TaskLists.GetTaskListById;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Domain.TaskLists.GetTaskListById
{
    public sealed class GetTaskListByIdQueryHandlerTests
    {
        private readonly Mock<ITaskListRepository> repositoryMock = new Mock<ITaskListRepository>();

        private readonly GetTaskListByIdQueryHandler testee;

        public GetTaskListByIdQueryHandlerTests() => testee = new GetTaskListByIdQueryHandler(repositoryMock.Object);

        [Test]
        public async Task GetTaskListById_GivenExistingTaskListId_ReturnsResponseWithTaskList()
        {
            var list = TaskList.New(1, "test list").AddItem("item");
            var expectedResponse = new GetTaskListByIdQueryResponse(list);

            _ = repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var result = await testee.ExecuteQuery(list.Id);

            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetTaskListById_GivenNonExistingTaskListId_ReturnsNull()
        {
            var id = TaskListId.Of(1);

            _ = repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var result = await testee.ExecuteQuery(id);

            Assert.IsNull(result);
        }
    }
}
