using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAC.Baseline.Web.Data;
using CAC.Baseline.Web.Model;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Infrastructure
{
    public abstract class TaskListEntryRepositoryTests
    {
        private const long OwningTaskListId = 1;

        protected abstract ITaskListEntryRepository Testee { get; }

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
        public async Task Store_GivenNonExistingEntry_StoresTaskList()
        {
            var entry = new TaskListEntry(1, OwningTaskListId, "task", false);

            await Testee.Store(entry);

            var storedEntry = await Testee.GetById(entry.Id);
            Assert.IsNotNull(storedEntry);
            Assert.AreEqual(entry.Description, storedEntry!.Description);
        }

        [Test]
        public async Task Store_GivenExistingEntry_ThrowsException()
        {
            var existingEntry = new TaskListEntry(1, OwningTaskListId, "task", false);
            await Testee.Store(existingEntry);

            var entry = new TaskListEntry(1, OwningTaskListId, "task", false);

            Assert.ThrowsAsync<ArgumentException>(() => Testee.Store(entry));
        }

        [Test]
        public async Task MarkEntryAsDone_GivenNonExistingEntry_ReturnsFalse()
        {
            var result = await Testee.MarkEntryAsDone(1);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task MarkEntryAsDone_GivenExistingEntry_UpdatesTaskListAndReturnsTrue()
        {
            var entry = new TaskListEntry(1, OwningTaskListId, "task", false);
            await Testee.Store(entry);

            var result = await Testee.MarkEntryAsDone(entry.Id);

            Assert.IsTrue(result);

            var storedEntry = await Testee.GetById(entry.Id);
            Assert.IsNotNull(storedEntry);
            Assert.IsTrue(storedEntry?.IsDone);
        }

        [Test]
        public async Task GetById_GivenNonExistingEntry_ReturnsNull()
        {
            var result = await Testee.GetById(1);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetById_GivenExistingEntry_ReturnsEntry()
        {
            var existingEntry = new TaskListEntry(1, OwningTaskListId, "task", false);
            await Testee.Store(existingEntry);

            var result = await Testee.GetById(existingEntry.Id);

            Assert.AreEqual(existingEntry.Id, result?.Id);
            Assert.AreEqual(existingEntry.Description, result?.Description);
        }

        [Test]
        public async Task GetEntriesForTaskList_ReturnsLists()
        {
            var existingEntry1 = new TaskListEntry(1, OwningTaskListId, "task 1", false);
            await Testee.Store(existingEntry1);

            var existingEntry2 = new TaskListEntry(2, OwningTaskListId, "task 2", false);
            await Testee.Store(existingEntry2);

            var result = await Testee.GetEntriesForTaskList(OwningTaskListId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(l => l.Id == existingEntry1.Id));
            Assert.IsTrue(result.Any(l => l.Id == existingEntry2.Id));
        }

        [Test]
        public async Task GetEntriesForTaskLists_ReturnsLists()
        {
            const long owningTaskListId2 = OwningTaskListId + 1;
            const long owningTaskListId3 = owningTaskListId2 + 1;

            var existingEntry1 = new TaskListEntry(1, OwningTaskListId, "task 1", false);
            await Testee.Store(existingEntry1);

            var existingEntry2 = new TaskListEntry(2, OwningTaskListId, "task 2", false);
            await Testee.Store(existingEntry2);

            var existingEntry3 = new TaskListEntry(3, owningTaskListId2, "task 1", false);
            await Testee.Store(existingEntry3);

            var result = await Testee.GetEntriesForTaskLists(new[] { OwningTaskListId, owningTaskListId2, owningTaskListId3 });

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[OwningTaskListId].Count);
            Assert.IsTrue(result[OwningTaskListId].Any(l => l.Id == existingEntry1.Id));
            Assert.IsTrue(result[OwningTaskListId].Any(l => l.Id == existingEntry2.Id));
            Assert.AreEqual(1, result[owningTaskListId2].Count);
            Assert.IsTrue(result[owningTaskListId2].Any(l => l.Id == existingEntry3.Id));
            Assert.AreEqual(0, result[owningTaskListId3].Count);
        }

        [Test]
        public async Task GetNumberOfEntriesForTaskList_ReturnsCount()
        {
            var existingEntry1 = new TaskListEntry(1, OwningTaskListId, "task 1", false);
            await Testee.Store(existingEntry1);

            var existingEntry2 = new TaskListEntry(2, OwningTaskListId, "task 2", false);
            await Testee.Store(existingEntry2);

            var result = await Testee.GetNumberOfEntriesForTaskList(OwningTaskListId);

            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task GetIdsOfAllTaskListsWithPendingEntries_ReturnsIds()
        {
            const long owningTaskListId2 = OwningTaskListId + 1;
            const long owningTaskListId3 = owningTaskListId2 + 1;

            var existingEntry1 = new TaskListEntry(1, OwningTaskListId, "task 1", false);
            await Testee.Store(existingEntry1);

            var existingEntry2 = new TaskListEntry(2, OwningTaskListId, "task 2", true);
            await Testee.Store(existingEntry2);

            var existingEntry3 = new TaskListEntry(3, owningTaskListId2, "task 1", false);
            await Testee.Store(existingEntry3);

            var existingEntry4 = new TaskListEntry(4, owningTaskListId3, "task 1", true);
            await Testee.Store(existingEntry4);

            var expectedResult = new[] { OwningTaskListId, owningTaskListId2 };
            var result = await Testee.GetIdsOfAllTaskListsWithPendingEntries();

            Assert.IsTrue(result.SequenceEqual(expectedResult));
        }
    }
}
