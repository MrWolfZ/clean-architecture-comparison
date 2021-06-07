using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Domain.TaskLists;
using CAC.CQS.Domain.TaskLists.CreateNewTaskList;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Domain.TaskLists.CreateNewTaskList
{
    public sealed class CreateNewTaskListCommandHandlerTests
    {
        private readonly Mock<ILogger<CreateNewTaskListCommandHandler>> loggerMock = new Mock<ILogger<CreateNewTaskListCommandHandler>>();
        private readonly Mock<ITaskListRepository> repositoryMock = new Mock<ITaskListRepository>();

        private readonly CreateNewTaskListCommandHandler testee;

        public CreateNewTaskListCommandHandlerTests() => testee = new CreateNewTaskListCommandHandler(repositoryMock.Object, loggerMock.Object);

        [Test]
        public async Task GivenValidName_StoresNewList()
        {
            const string name = "test list";
            var expectedId = TaskListId.Of(1);
            var expectedList = TaskList.New(expectedId, name);

            _ = repositoryMock.Setup(r => r.GenerateId()).ReturnsAsync(expectedId);

            var response = await testee.ExecuteCommand(new CreateNewTaskListCommand { Name = name });

            Assert.AreEqual(expectedId, response.Id);

            repositoryMock.Verify(r => r.GenerateId(), Times.Once);
            repositoryMock.Verify(r => r.Upsert(expectedList));
        }

        [Test]
        public void GivenInvalidName_ThrowsDomainException()
        {
            _ = repositoryMock.Setup(r => r.GenerateId()).ReturnsAsync(1);

            _ = Assert.ThrowsAsync<DomainInvariantViolationException>(() => testee.ExecuteCommand(new CreateNewTaskListCommand { Name = " " }));
        }
    }
}
