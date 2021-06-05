using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAC.Baseline.Web.Data;
using CAC.Baseline.Web.Model;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Infrastructure
{
    public abstract class TaskListRepositoryTests
    {
        private const long OwnerId = 1;

        protected abstract ITaskListRepository Testee { get; }

        [Test]
        public async Task GenerateId_ReturnsNewIdOnEachCall()
        {
            var generatedIds = new HashSet<long>();

            for (var i = 0; i < 100; i += 1)
            {
                var id = await Testee.GenerateId();
                Assert.IsFalse(generatedIds.Contains(id));
                generatedIds.Add(id);
            }
        }

        [Test]
        public async Task Store_GivenNonExistingTaskList_StoresTaskList()
        {
            var list = new TaskList(1, OwnerId, "test");

            await Testee.Store(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
        }

        [Test]
        public async Task Store_GivenExistingTaskList_ThrowsException()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var list = new TaskList(1, OwnerId, "test");

            Assert.ThrowsAsync<ArgumentException>(() => Testee.Store(list));
        }

        [Test]
        public async Task Store_GivenNewTaskListWithExistingNameAndSameOwner_ThrowsException()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var list = new TaskList(2, OwnerId, existingList.Name);
            Assert.ThrowsAsync<ArgumentException>(() => Testee.Store(list));
        }

        [Test]
        public async Task Store_GivenNewTaskListWithExistingNameAndDifferentOwner_StoresTaskList()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var list = new TaskList(2, OwnerId + 1, existingList.Name);
            await Testee.Store(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
        }

        [Test]
        public async Task DeleteById_GivenNonExistingTaskList_ReturnsFalse()
        {
            var result = await Testee.DeleteById(1);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskList_DeletesListAndReturnsTrue()
        {
            var list = new TaskList(1, OwnerId, "test");
            await Testee.Store(list);

            var result = await Testee.DeleteById(list.Id);
            Assert.IsTrue(result);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNull(storedList);
        }

        [Test]
        public async Task Exists_GivenNonExistingTaskList_ReturnsFalse()
        {
            var result = await Testee.Exists(1);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task Exists_GivenExistingTaskList_ReturnsTrue()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var result = await Testee.Exists(existingList.Id);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskList_ReturnsNull()
        {
            var result = await Testee.GetById(1);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetById_GivenExistingTaskList_ReturnsList()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var result = await Testee.GetById(existingList.Id);

            Assert.AreEqual(existingList.Id, result?.Id);
            Assert.AreEqual(existingList.Name, result?.Name);
        }

        [Test]
        public async Task GetByIds_GivenNonExistingTaskList_ReturnsEmptyCollection()
        {
            var result = await Testee.GetByIds(new[] { 1L });

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetByIds_GivenExistingTaskLists_ReturnsLists()
        {
            var existingList1 = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList1);
            
            var existingList2 = new TaskList(2, OwnerId, "test 2");
            await Testee.Store(existingList2);

            var result = await Testee.GetByIds(new[] { existingList1.Id, existingList2.Id });

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(l => l.Id == existingList1.Id));
            Assert.IsTrue(result.Any(l => l.Id == existingList2.Id));
        }

        [Test]
        public async Task GetByIds_GivenExistingAndNonExistingTaskLists_ReturnsExistingLists()
        {
            var existingList1 = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList1);

            var result = await Testee.GetByIds(new[] { existingList1.Id, 2 });

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(l => l.Id == existingList1.Id));
        }

        [Test]
        public async Task GetOwnerId_GivenNonExistingTaskList_ReturnsNull()
        {
            var result = await Testee.GetOwnerId(1);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetOwnerId_GivenExistingTaskList_ReturnsOwnerId()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Store(existingList);

            var result = await Testee.GetOwnerId(existingList.Id);

            Assert.AreEqual(existingList.OwnerId, result);
        }

        [Test]
        public async Task GetAll_GivenNoStoredTaskLists_ReturnsEmptyCollection()
        {
            var lists = await Testee.GetAll();
            Assert.AreEqual(0, lists.Count);
        }

        [Test]
        public async Task GetAll_GivenStoredTaskLists_ReturnsCollectionOfLists()
        {
            var list1 = new TaskList(1, OwnerId, "test 1");
            await Testee.Store(list1);

            var list2 = new TaskList(2, OwnerId, "test 2");
            await Testee.Store(list2);

            var lists = await Testee.GetAll();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetNumberOfTaskListsByOwner_GivenStoredTaskLists_ReturnsCorrectCounts()
        {
            const long ownerId2 = OwnerId + 1;
            const long ownerId3 = OwnerId + 2;

            var list1 = new TaskList(1, OwnerId, "test 1");
            await Testee.Store(list1);

            var list2 = new TaskList(2, OwnerId, "test 2");
            await Testee.Store(list2);

            var list3 = new TaskList(3, ownerId2, "test");
            await Testee.Store(list3);

            Assert.AreEqual(2, await Testee.GetNumberOfTaskListsByOwner(OwnerId));
            Assert.AreEqual(1, await Testee.GetNumberOfTaskListsByOwner(ownerId2));
            Assert.AreEqual(0, await Testee.GetNumberOfTaskListsByOwner(ownerId3));
        }
    }
}
