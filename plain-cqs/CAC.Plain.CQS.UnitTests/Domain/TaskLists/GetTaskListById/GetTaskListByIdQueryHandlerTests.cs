using System.Threading.Tasks;
using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Domain.TaskLists.GetTaskListById;
using Moq;
using NUnit.Framework;

namespace CAC.Plain.CQS.UnitTests.Domain.TaskLists.GetTaskListById
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

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var result = await testee.ExecuteQuery(list.Id);

            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public async Task GetTaskListById_GivenNonExistingTaskListId_ReturnsNull()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var result = await testee.ExecuteQuery(id);

            Assert.IsNull(result);
        }
    }
}
