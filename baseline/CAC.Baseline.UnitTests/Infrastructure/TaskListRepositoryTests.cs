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
        public async Task Upsert_GivenNonExistingTaskList_StoresTaskList()
        {
            var list = new TaskList(1, OwnerId, "test");
            list.Entries.Add(new TaskListEntry("task", false));

            await Testee.Upsert(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
            Assert.IsTrue(list.Entries.SequenceEqual(storedList.Entries));
        }

        [Test]
        public async Task Upsert_GivenExistingTaskList_StoresTaskList()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Upsert(existingList);

            var list = new TaskList(1, OwnerId, "test");
            list.Entries.Add(new TaskListEntry("task", false));

            await Testee.Upsert(list);

            var lists = await Testee.GetAll();
            Assert.AreEqual(1, lists.Count);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
            Assert.IsTrue(list.Entries.SequenceEqual(storedList.Entries));
        }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndSameOwner_ThrowsException()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Upsert(existingList);

            var list = new TaskList(2, OwnerId, existingList.Name);
            Assert.ThrowsAsync<ArgumentException>(() => Testee.Upsert(list));
        }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndDifferentOwner_StoresTaskList()
        {
            var existingList = new TaskList(1, OwnerId, "test");
            await Testee.Upsert(existingList);

            var list = new TaskList(2, OwnerId + 1, existingList.Name);
            await Testee.Upsert(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
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
            await Testee.Upsert(list1);

            var list2 = new TaskList(2, OwnerId, "test 2");
            list2.Entries.Add(new TaskListEntry("task", false));
            await Testee.Upsert(list2);

            var lists = await Testee.GetAll();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenStoredTaskLists_ReturnsCollectionOfListsWithPendingEntries()
        {
            var list1 = new TaskList(1, OwnerId, "test 1");
            list1.Entries.Add(new TaskListEntry("task 1", true));
            list1.Entries.Add(new TaskListEntry("task 2", false));

            var list2 = new TaskList(2, OwnerId, "test 2");
            list2.Entries.Add(new TaskListEntry("task 1", false));

            var list3 = new TaskList(3, OwnerId, "test 3");
            list3.Entries.Add(new TaskListEntry("task 1", true));

            await Testee.Upsert(list1);
            await Testee.Upsert(list2);
            await Testee.Upsert(list3);

            var lists = await Testee.GetAllWithPendingEntries();
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
            await Testee.Upsert(list1);

            var list2 = new TaskList(2, OwnerId, "test 2");
            list2.Entries.Add(new TaskListEntry("task", false));
            await Testee.Upsert(list2);

            var list3 = new TaskList(3, ownerId2, "test");
            await Testee.Upsert(list3);

            Assert.AreEqual(2, await Testee.GetNumberOfTaskListsByOwner(OwnerId));
            Assert.AreEqual(1, await Testee.GetNumberOfTaskListsByOwner(ownerId2));
            Assert.AreEqual(0, await Testee.GetNumberOfTaskListsByOwner(ownerId3));
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
            await Testee.Upsert(list);

            var result = await Testee.DeleteById(list.Id);
            Assert.IsTrue(result);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNull(storedList);
        }
    }
}
