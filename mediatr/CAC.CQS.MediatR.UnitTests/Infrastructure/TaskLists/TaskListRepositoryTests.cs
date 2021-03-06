using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.Core.TestUtilities;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using CAC.CQS.MediatR.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.Infrastructure.TaskLists
{
    public abstract class TaskListRepositoryTests : AggregateRepositoryTestBase<TaskList, TaskListId>
    {
        private const long OwnerId = 1;
        private long taskListEntryIdCounter;

        private long taskListIdCounter;

        protected abstract override ITaskListRepository Testee { get; }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndSameOwner_ThrowsException()
        {
            var existingList = CreateTaskList();
            existingList = await Testee.Upsert(existingList);

            var list = TaskList.FromRawData(2, OwnerId, true, existingList.Name, ValueList<TaskListEntry>.Empty, SystemTime.Now, null);
            _ = Assert.ThrowsAsync<UniquenessConstraintViolationException>(() => Testee.Upsert(list));
        }

        [Test]
        public async Task Upsert_GivenNewTaskListWithExistingNameAndDifferentOwner_StoresTaskList()
        {
            var existingList = CreateTaskList();
            existingList = await Testee.Upsert(existingList);

            var list = CreateTaskList(ownerId: OwnerId + 1, name: existingList.Name);
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
            var list1 = CreateTaskList();
            list1 = await Testee.Upsert(list1);

            var list2 = CreateTaskList(1);

            list2 = await Testee.Upsert(list2);

            var lists = await Testee.GetAll();
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllByOwner_GivenNoTaskListsForOwner_ReturnsEmptyCollection()
        {
            var lists = await Testee.GetAllByOwner(OwnerId);
            Assert.AreEqual(0, lists.Count);
        }

        [Test]
        public async Task GetAllByOwner_GivenTaskListsForOwner_ReturnsCollectionOfLists()
        {
            var list1 = CreateTaskList();
            list1 = await Testee.Upsert(list1);

            var list2 = CreateTaskList(1);

            list2 = await Testee.Upsert(list2);

            var lists = await Testee.GetAllByOwner(OwnerId);
            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == list1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == list2.Name));
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenStoredTaskLists_ReturnsCollectionOfListsWithPendingEntries()
        {
            var list1 = CreateTaskList(2);
            list1 = list1.MarkEntryAsDone(list1.Entries.First().Id);

            var list2 = CreateTaskList(1);

            var list3 = CreateTaskList(1);
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
            const long ownerId2 = OwnerId + 1;
            const long ownerId3 = OwnerId + 2;

            var list1 = CreateTaskList();
            _ = await Testee.Upsert(list1);

            var list2 = CreateTaskList(1);
            _ = await Testee.Upsert(list2);

            var list3 = CreateTaskList(ownerId: ownerId2);
            _ = await Testee.Upsert(list3);

            Assert.AreEqual(2, await Testee.GetNumberOfTaskListsByOwner(OwnerId));
            Assert.AreEqual(1, await Testee.GetNumberOfTaskListsByOwner(ownerId2));
            Assert.AreEqual(0, await Testee.GetNumberOfTaskListsByOwner(ownerId3));
        }

        protected override TaskList CreateAggregate() => CreateTaskList(1);

        protected override TaskList UpdateAggregate(TaskList aggregate)
        {
            var entry = CreateEntry();
            var entries = aggregate.Entries.Add(entry);
            return TaskList.FromRawData(aggregate.Id, aggregate.OwnerId, true, aggregate.Name, entries, SystemTime.Now, null);
        }

        private TaskList CreateTaskList(int numberOfEntries = 0, UserId? ownerId = null, string? name = null)
        {
            var listId = ++taskListIdCounter;
            var entries = ValueList.ToValueList(Enumerable.Range(1, numberOfEntries).Select(_ => CreateEntry()));
            return TaskList.FromRawData(listId, ownerId ?? OwnerId, true, name ?? $"list {listId}", entries, SystemTime.Now, null);
        }

        private TaskListEntry CreateEntry()
        {
            var entryId = ++taskListEntryIdCounter;
            return TaskListEntry.FromRawData(entryId, $"task {entryId}", false);
        }
    }
}