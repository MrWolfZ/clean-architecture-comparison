using System.Linq;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Basic.UnitTests.Domain.TaskListAggregate;
using CAC.Basic.UnitTests.Domain.UserAggregate;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.Core.TestUtilities;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Infrastructure.TaskLists
{
    public abstract class TaskListRepositoryTests : AggregateRepositoryTestBase<TaskList, TaskListId>
    {
        protected abstract override ITaskListRepository Testee { get; }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndSameOwner_ThrowsException()
        {
            var existingList = new TaskListBuilder().Build();
            existingList = await Testee.Upsert(existingList);

            var list = new TaskListBuilder().WithName(existingList.Name).Build();
            _ = Assert.ThrowsAsync<UniquenessConstraintViolationException>(() => Testee.Upsert(list));
        }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndDifferentOwner_StoresTaskList()
        {
            var existingList = new TaskListBuilder().Build();
            existingList = await Testee.Upsert(existingList);
            var list = new TaskListBuilder().WithOwner(new UserBuilder().Build()).WithName(existingList.Name).Build();
            list = await Testee.Upsert(list);

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
            var list1 = new TaskListBuilder().Build();
            list1 = await Testee.Upsert(list1);

            var list2 = new TaskListBuilder().WithPendingEntries(1).Build();

            list2 = await Testee.Upsert(list2);

            var lists = await Testee.GetAll();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllByOwner_GivenNoTaskListsForOwner_ReturnsEmptyCollection()
        {
            var lists = await Testee.GetAllByOwner(1);
            Assert.AreEqual(0, lists.Count);
        }

        [Test]
        public async Task GetAllByOwner_GivenTaskListsForOwner_ReturnsCollectionOfLists()
        {
            var list1 = new TaskListBuilder().Build();
            list1 = await Testee.Upsert(list1);

            var list2 = new TaskListBuilder().WithPendingEntries(1).Build();

            list2 = await Testee.Upsert(list2);

            var lists = await Testee.GetAllByOwner(1);
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenStoredTaskLists_ReturnsCollectionOfListsWithPendingEntries()
        {
            var list1 = new TaskListBuilder().WithPendingEntries(2).Build();
            list1 = list1.MarkEntryAsDone(list1.Entries.First().Id);

            var list2 = new TaskListBuilder().WithPendingEntries(1).Build();

            var list3 = new TaskListBuilder().WithPendingEntries(1).Build();
            list3 = list3.MarkEntryAsDone(list3.Entries.First().Id);

            list1 = await Testee.Upsert(list1);
            list2 = await Testee.Upsert(list2);
            _ = await Testee.Upsert(list3);

            var lists = await Testee.GetAllWithPendingEntries();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetNumberOfTaskListsByOwner_GivenStoredTaskLists_ReturnsCorrectCounts()
        {
            var owner2 = new UserBuilder().Build();
            var owner3 = new UserBuilder().Build();

            var list1 = new TaskListBuilder().Build();
            _ = await Testee.Upsert(list1);

            var list2 = new TaskListBuilder().WithPendingEntries(1).Build();
            _ = await Testee.Upsert(list2);

            var list3 = new TaskListBuilder().WithOwner(owner2).Build();
            _ = await Testee.Upsert(list3);

            Assert.AreEqual(2, await Testee.GetNumberOfTaskListsByOwner(list1.OwnerId));
            Assert.AreEqual(1, await Testee.GetNumberOfTaskListsByOwner(list3.OwnerId));
            Assert.AreEqual(0, await Testee.GetNumberOfTaskListsByOwner(owner3.Id));
        }

        protected override TaskList CreateAggregate() => new TaskListBuilder().WithPendingEntries(1).Build();

        protected override TaskList UpdateAggregate(TaskList aggregate)
        {
            var entry = new TaskListEntryBuilder().Build();
            var entries = aggregate.Entries.Add(entry);
            return TaskList.FromRawData(aggregate.Id, aggregate.OwnerId, true, aggregate.Name, entries, SystemTime.Now, null);
        }
    }
}
