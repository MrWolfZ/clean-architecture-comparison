using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.Plain.Domain.TaskLists;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Plain.UnitTests.Domain.TaskLists
{
    public sealed class TaskListServiceTests
    {
        private readonly Mock<ILogger<TaskListService>> loggerMock = new Mock<ILogger<TaskListService>>();
        private readonly Mock<ITaskListRepository> repositoryMock = new Mock<ITaskListRepository>();

        private readonly ITaskListService testee;

        public TaskListServiceTests() => testee = new TaskListService(repositoryMock.Object, loggerMock.Object);

        [Test]
        public async Task CreateNewTaskList_GivenValidName_StoresNewList()
        {
            const string name = "test list";
            var expectedId = TaskListId.Of(1);
            var expectedList = TaskList.New(expectedId, name);

            repositoryMock.Setup(r => r.GenerateId()).ReturnsAsync(expectedId);

            var taskListId = await testee.CreateNewTaskList(name);

            Assert.AreEqual(expectedId, taskListId);

            repositoryMock.Verify(r => r.GenerateId(), Times.Once);
            repositoryMock.Verify(r => r.Upsert(expectedList));
        }

        [Test]
        public void CreateNewTaskList_GivenInvalidName_ThrowsDomainException()
        {
            repositoryMock.Setup(r => r.GenerateId()).ReturnsAsync(1);

            Assert.ThrowsAsync<DomainValidationException>(() => testee.CreateNewTaskList(" "));
        }

        [Test]
        public async Task AddItemToTaskList_GivenExistingEmptyTaskListIdAndValidDescription_AddsItemToListAndStoresList()
        {
            const string description = "new item";
            var list = TaskList.New(1, "test list");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var wasFound = await testee.AddItemToTaskList(list.Id, description);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items.Single().Description == description)));
        }

        [Test]
        public async Task AddItemToTaskList_GivenExistingNonEmptyTaskListIdAndValidDescription_AddsItemToListAndStoresList()
        {
            const string description = "new item";
            var list = TaskList.New(1, "test list").AddItem("existing item");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var wasFound = await testee.AddItemToTaskList(list.Id, description);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items.Count == 2 && l.Items[1].Description == description)));
        }

        [Test]
        public async Task AddItemToTaskList_GivenNonExistingTaskListId_DoesNotUpdateListAndReturnsFalse()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var wasFound = await testee.AddItemToTaskList(id, "new item");

            Assert.IsFalse(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }

        [Test]
        public async Task GetTaskListById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var list = TaskList.New(1, "test list").AddItem("item");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var result = await testee.GetTaskListById(list.Id);

            Assert.AreSame(list, result);
        }

        [Test]
        public async Task GetTaskListById_GivenNonExistingTaskListId_ReturnsNull()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var result = await testee.GetTaskListById(id);

            Assert.IsNull(result);
        }

        [Test]
        public async Task MarkTaskListItemAsDone_GivenExistingTaskListIdAndValidItemIndex_MarksItemAsDoneAndStoresListAndReturnsTrue()
        {
            var list = TaskList.New(1, "test list").AddItem("item 1").AddItem("item 2");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            var wasFound = await testee.MarkTaskListItemAsDone(list.Id, 1);

            Assert.IsTrue(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.Is<TaskList>(l => l.Items[1].IsDone)));
        }

        [Test]
        public async Task MarkTaskListItemAsDone_GivenNonExistingTaskListId_ReturnsFalse()
        {
            var id = TaskListId.Of(1);

            repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(() => null);

            var wasFound = await testee.MarkTaskListItemAsDone(id, 1);

            Assert.IsFalse(wasFound);

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }

        [Test]
        public void MarkTaskListItemAsDone_GivenExistingTaskListIdAndInvalidItemIndex_ThrowsException()
        {
            var list = TaskList.New(1, "test list").AddItem("item 1").AddItem("item 2");

            repositoryMock.Setup(r => r.GetById(list.Id)).ReturnsAsync(list);

            Assert.ThrowsAsync<DomainValidationException>(() => testee.MarkTaskListItemAsDone(list.Id, 2));

            repositoryMock.Verify(r => r.Upsert(It.IsAny<TaskList>()), Times.Never);
        }
    }
}
