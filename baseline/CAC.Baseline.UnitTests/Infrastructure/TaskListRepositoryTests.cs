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
            var list = new TaskList(1, "test");
            list.AddItem("task");

            await Testee.Upsert(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
            Assert.IsTrue(list.Items.SequenceEqual(storedList.Items));
        }

        [Test]
        public async Task Upsert_GivenExistingTaskList_StoresTaskList()
        {
            var existingList = new TaskList(1, "test");
            await Testee.Upsert(existingList);

            var list = new TaskList(1, "test");
            list.AddItem("task");

            await Testee.Upsert(list);

            var lists = await Testee.GetAll();
            Assert.AreEqual(1, lists.Count);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNotNull(storedList);
            Assert.AreEqual(list.Name, storedList!.Name);
            Assert.IsTrue(list.Items.SequenceEqual(storedList.Items));
        }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingName_ThrowsException()
        {
            var existingList = new TaskList(1, "test");
            await Testee.Upsert(existingList);

            var list = new TaskList(2, "test");
            Assert.ThrowsAsync<ArgumentException>(() => Testee.Upsert(list));
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
            var list1 = new TaskList(1, "test 1");
            await Testee.Upsert(list1);

            var list2 = new TaskList(2, "test 2");
            list2.AddItem("task");
            await Testee.Upsert(list2);

            var lists = await Testee.GetAll();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllWithPendingItems_GivenStoredTaskLists_ReturnsCollectionOfListsWithPendingItems()
        {
            var list1 = new TaskList(1, "test 1");
            list1.AddItem("task 1");
            list1.AddItem("task 2");
            list1.MarkItemAsDone(0);

            var list2 = new TaskList(2, "test 2");
            list2.AddItem("task 1");

            var list3 = new TaskList(3, "test 3");
            list3.AddItem("task 1");
            list3.MarkItemAsDone(0);

            await Testee.Upsert(list1);
            await Testee.Upsert(list2);
            await Testee.Upsert(list3);

            var lists = await Testee.GetAllWithPendingItems();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }
    }
}
