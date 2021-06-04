using System;
using System.Linq;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Application.Users;
using CAC.Basic.Domain.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Basic.Infrastructure.Users;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Domain.TaskLists
{
    public sealed class TaskListServiceTests
    {
        private const long PremiumOwnerId = 1;
        private const long NonPremiumOwnerId = 2;
        
        private readonly ITaskListRepository repository = new InMemoryTaskListRepository();
        private readonly IUserRepository userRepository = new InMemoryUserRepository();
        private readonly Mock<ITaskListStatisticsService> statisticsServiceMock = new Mock<ITaskListStatisticsService>();

        private readonly ITaskListService testee;

        public TaskListServiceTests()
        {
            testee = new TaskListService(repository,
                                         userRepository,
                                         statisticsServiceMock.Object,
                                         Mock.Of<ILogger<TaskListService>>());
        }

        [Test]
        public async Task CreateNewTaskList_GivenValidName_StoresTaskListAndReturnsCreatedTaskList()
        {
            const string name = "test";
            
            var result = await testee.CreateNewTaskList(PremiumOwnerId, name);

            Assert.IsNotNull(result);
            Assert.AreEqual(name, result?.Name);

            var storedTaskList = await repository.GetById(result!.Id);
            Assert.AreSame(result, storedTaskList);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonExistingOwnerId_ReturnsNull()
        {
            var result = await testee.CreateNewTaskList(99, "test");

            Assert.IsNull(result);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void CreateNewTaskList_GivenInvalidName_ThrowsArgumentException(string name)
        {
            Assert.CatchAsync<ArgumentException>(() => testee.CreateNewTaskList(PremiumOwnerId, name));
        }

        [Test]
        public void CreateNewTaskList_GivenNameWithTooManyCharacters_ThrowsArgumentException()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", TaskListService.MaxTaskListNameLength + 1));
            
            Assert.ThrowsAsync<ArgumentException>(() => testee.CreateNewTaskList(PremiumOwnerId, name));
        }

        [Test]
        public async Task CreateNewTaskList_GivenPremiumOwnerWithExistingTaskList_ReturnsNonNull()
        {
            await repository.Upsert(new TaskList(99, PremiumOwnerId, "existing"));
            
            var result = await testee.CreateNewTaskList(PremiumOwnerId, "test");

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonPremiumOwnerWithExistingTaskList_ThrowsArgumentException()
        {
            await repository.Upsert(new TaskList(99, NonPremiumOwnerId, "existing"));
            
            Assert.ThrowsAsync<ArgumentException>(() => testee.CreateNewTaskList(NonPremiumOwnerId, "test"));
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_UpdatesStatistics()
        {
            var result = await testee.CreateNewTaskList(PremiumOwnerId, "test");
            
            statisticsServiceMock.Verify(s => s.OnTaskListCreated(result!));
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_ReturnsUpdatedTaskList()
        {
            const string taskDescription = "test";
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await repository.Upsert(taskList);

            var result = await testee.AddTaskToList(taskList.Id, taskDescription);

            Assert.IsNotNull(result);
            Assert.AreEqual(taskDescription, result?.Entries.Single().Description);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ThrowsArgumentException(string description)
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            
            await repository.Upsert(taskList);
            
            Assert.CatchAsync<ArgumentException>(() => testee.AddTaskToList(taskList.Id, description));
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_ThrowsArgumentException()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            var description = string.Join(string.Empty, Enumerable.Repeat("a", TaskListService.MaxTaskDescriptionLength + 1));

            await repository.Upsert(taskList);

            Assert.ThrowsAsync<ArgumentException>(() => testee.AddTaskToList(taskList.Id, description));
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNull()
        {
            var result = await testee.AddTaskToList(1, "test");

            Assert.IsNull(result);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithLessThanFiveEntriesAndNonPremiumOwner_ReturnsNonNull()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");
            taskList.AddEntry("task 3");
            taskList.AddEntry("task 4");

            await repository.Upsert(taskList);

            var result = await testee.AddTaskToList(taskList.Id, "test");

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithFiveEntriesAndNonPremiumOwner_ThrowsArgumentException()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");
            taskList.AddEntry("task 3");
            taskList.AddEntry("task 4");
            taskList.AddEntry("task 5");

            await repository.Upsert(taskList);
            
            Assert.ThrowsAsync<ArgumentException>(() => testee.AddTaskToList(taskList.Id, "test"));
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await repository.Upsert(taskList);

            var result = await testee.AddTaskToList(taskList.Id, "test");
            
            statisticsServiceMock.Verify(s => s.OnTaskAddedToList(result!, 0));
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidEntryIndex_ReturnsUpdatedTaskList()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await repository.Upsert(taskList);

            var result = await testee.MarkTaskAsDone(taskList.Id, 1);

            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Entries.ElementAt(1).IsDone);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingEntryIndex_ThrowsArgumentException()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await repository.Upsert(taskList);
            
            Assert.ThrowsAsync<ArgumentException>(() => testee.MarkTaskAsDone(taskList.Id, 2));
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNull()
        {
            var result = await testee.MarkTaskAsDone(1, 1);

            Assert.IsNull(result);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task");

            await repository.Upsert(taskList);

            var result = await testee.MarkTaskAsDone(taskList.Id, 0);
            
            statisticsServiceMock.Verify(s => s.OnTaskMarkedAsDone(result!, 0));
        }

        [Test]
        public async Task GetAll_GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = new TaskList(1, PremiumOwnerId, "test 1");
            taskList1.AddEntry("task 1");
            taskList1.AddEntry("task 2");

            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");

            var expectedLists = new[] { taskList1, taskList2 };
            
            await repository.Upsert(taskList1);
            await repository.Upsert(taskList2);

            var lists = await testee.GetAll();

            Assert.IsTrue(lists.SequenceEqual(expectedLists));
        }

        [Test]
        public async Task GetById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await repository.Upsert(taskList);

            var list = await testee.GetById(taskList.Id);

            Assert.AreSame(taskList, list);
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskListId_ReturnsNull()
        {
            var list = await testee.GetById(1);

            Assert.IsNull(list);
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenExistingTaskLists_ReturnsTaskListsWithPendingEntries()
        {
            var taskList1 = new TaskList(1, PremiumOwnerId, "test 1");
            taskList1.AddEntry("task 1");
            taskList1.AddEntry("task 2");
            taskList1.MarkEntryAsDone(0);

            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");
            taskList2.AddEntry("task 1");

            var taskList3 = new TaskList(3, PremiumOwnerId, "test 3");
            taskList3.AddEntry("task 1");
            taskList3.MarkEntryAsDone(0);
            
            var expectedLists = new[] { taskList1, taskList2 };
            
            await repository.Upsert(taskList1);
            await repository.Upsert(taskList2);
            await repository.Upsert(taskList3);

            var lists = await testee.GetAllWithPendingEntries();

            Assert.IsTrue(lists.SequenceEqual(expectedLists));
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskListId_DeletesTaskListAndReturnsTrue()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await repository.Upsert(taskList);

            var wasDeleted = await testee.DeleteById(taskList.Id);

            Assert.IsTrue(wasDeleted);
            
            var result = await repository.GetById(taskList.Id);
            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteById_GivenNonExistingTaskListId_ReturnsFalse()
        {
            var wasDeleted = await testee.DeleteById(1);

            Assert.IsFalse(wasDeleted);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await repository.Upsert(taskList);

            await testee.DeleteById(taskList.Id);

            statisticsServiceMock.Verify(s => s.OnTaskListDeleted(taskList.Id));
        }
    }
}
