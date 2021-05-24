using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Domain.TaskLists.MarkTaskAsDone;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Plain.CQS.UnitTests.Domain.TaskLists.MarkTaskAsDone
{
    public sealed class MarkTaskAsDoneCommandHandlerTests
    {
        private readonly Mock<ILogger<MarkTaskAsDoneCommandHandler>> loggerMock = new Mock<ILogger<MarkTaskAsDoneCommandHandler>>();
        private readonly Mock<ITaskListRepository> repositoryMock = new Mock<ITaskListRepository>();

        private readonly MarkTaskAsDoneCommandHandler testee;

        public MarkTaskAsDoneCommandHandlerTests() => testee = new MarkTaskAsDoneCommandHandler(repositoryMock.Object, loggerMock.Object);

        [Test]
        public async Task GivenExistingTaskListIdAndValidItemIndex_MarksItemAsDoneAndStoresListAndReturnsTrue()
        {
            var list = TaskList.New(1, "test list").AddItem("item 1").AddItem("item 2");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var command = new MarkTaskAsDoneCommand(list.Id) { ItemIdx = 1 };
            var wasFound = await testee.ExecuteCommand(command);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items[1].IsDone)));
        }

        [Test]
        public async Task GivenNonExistingTaskListId_ReturnsFalse()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var command = new MarkTaskAsDoneCommand(id) { ItemIdx = 1 };
            var wasFound = await testee.ExecuteCommand(command);

            Assert.IsFalse(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }

        [Test]
        public void GivenExistingTaskListIdAndInvalidItemIndex_ThrowsException()
        {
            var list = TaskList.New(1, "test list").AddItem("item 1").AddItem("item 2");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var command = new MarkTaskAsDoneCommand(list.Id) { ItemIdx = 2 };
            Assert.ThrowsAsync<DomainValidationException>(() => testee.ExecuteCommand(command));

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }
    }
}
