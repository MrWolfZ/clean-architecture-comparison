using System.Linq;
using System.Threading.Tasks;
using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Domain.TaskLists.AddTaskToList;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Plain.CQS.UnitTests.Domain.TaskLists.AddTaskToList
{
    public sealed class AddTaskToListCommandHandlerTests
    {
        private readonly Mock<ILogger<AddTaskToListCommandHandler>> loggerMock = new Mock<ILogger<AddTaskToListCommandHandler>>();
        private readonly Mock<ITaskListRepository> repositoryMock = new Mock<ITaskListRepository>();

        private readonly AddTaskToListCommandHandler testee;

        public AddTaskToListCommandHandlerTests() => testee = new AddTaskToListCommandHandler(repositoryMock.Object, loggerMock.Object);

        [Test]
        public async Task GivenExistingEmptyTaskListIdAndValidDescription_AddsItemToListAndStoresList()
        {
            const string description = "new item";
            var list = TaskList.New(1, "test list");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var command = new AddTaskToListCommand(list.Id) { TaskDescription = description };
            var wasFound = await testee.ExecuteCommand(command);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items.Single().Description == description)));
        }

        [Test]
        public async Task GivenExistingNonEmptyTaskListIdAndValidDescription_AddsItemToListAndStoresList()
        {
            const string description = "new item";
            var list = TaskList.New(1, "test list").AddItem("existing item");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var command = new AddTaskToListCommand(list.Id) { TaskDescription = description };
            var wasFound = await testee.ExecuteCommand(command);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items.Count == 2 && l.Items[1].Description == description)));
        }

        [Test]
        public async Task GivenNonExistingTaskListId_DoesNotUpdateListAndReturnsFalse()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var command = new AddTaskToListCommand(id) { TaskDescription = "task" };
            var wasFound = await testee.ExecuteCommand(command);

            Assert.IsFalse(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }
    }
}
