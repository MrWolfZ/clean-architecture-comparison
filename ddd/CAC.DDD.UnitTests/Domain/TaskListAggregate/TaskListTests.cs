using CAC.Core.Domain.Exceptions;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Domain.TaskListAggregate
{
    public sealed class TaskListTests
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        private static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);

        [Test]
        public void New_GivenValidName_CreatesList()
        {
            var id = TaskListId.Of(1);
            const string name = "list";
            var list = TaskList.ForOwner(PremiumOwner, id, name, 0);
            Assert.AreEqual(id, list.Id);
            Assert.AreEqual(PremiumOwner.Id, list.OwnerId);
            Assert.AreEqual(name, list.Name);
            Assert.IsEmpty(list.Entries);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void New_GivenEmptyName_ThrowsException(string name)
        {
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskList.ForOwner(PremiumOwner, 1, name, 0));
        }

        [Test]
        public void New_GivenPremiumOwnerWithExistingList_CreatesList()
        {
            Assert.DoesNotThrow(() => TaskList.ForOwner(PremiumOwner, 1, "list", 1));
        }

        [Test]
        public void New_GivenNonPremiumOwnerWithExistingList_ThrowsException()
        {
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskList.ForOwner(NonPremiumOwner, 1, "list", 1));
        }

        [Test]
        public void AddEntry_GivenValidDescription_AddsPendingEntryToList()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            const string description = "task";

            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, description);
            var updatedList = list.AddEntry(entry);

            Assert.Contains(entry, updatedList.Entries);
        }

        [Test]
        public void AddEntry_GivenMultipleValidDescriptions_AddsPendingEntriesToList()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            var entry1 = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task 1");
            var entry2 = TaskListEntry.ForAddingToTaskList(list.Id, 2, "task 2");
            var updatedList = list.AddEntry(entry1).AddEntry(entry2);

            Assert.AreEqual(2, updatedList.Entries.Count);
            Assert.Contains(entry1, updatedList.Entries);
            Assert.Contains(entry2, updatedList.Entries);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void TaskListEntry_ForAddingToTaskList_GivenInvalidDescription_ThrowsException(string description)
        {
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskListEntry.ForAddingToTaskList(1, 1, description));
        }

        [Test]
        public void AddEntry_GivenPremiumOwnerAndListWithEntriesAtNonPremiumLimit_AddsPendingEntriesToList()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            for (var i = 1; i <= TaskList.NonPremiumUserTaskEntryCountLimit; i += 1)
            {
                var entry = TaskListEntry.ForAddingToTaskList(list.Id, i, $"task {i}");
                list = list.AddEntry(entry);
            }

            var newEntry = TaskListEntry.ForAddingToTaskList(list.Id, TaskList.NonPremiumUserTaskEntryCountLimit + 1, "new task");
            Assert.DoesNotThrow(() => list.AddEntry(newEntry));
        }

        [Test]
        public void AddEntry_GivenNonPremiumOwnerAndListWithEntriesAtNonPremiumLimit_ThrowsException()
        {
            var list = TaskList.ForOwner(NonPremiumOwner, 1, "list", 0);

            for (var i = 1; i <= TaskList.NonPremiumUserTaskEntryCountLimit; i += 1)
            {
                var entry = TaskListEntry.ForAddingToTaskList(list.Id, i, $"task {i}");
                list = list.AddEntry(entry);
            }

            var newEntry = TaskListEntry.ForAddingToTaskList(list.Id, TaskList.NonPremiumUserTaskEntryCountLimit + 1, "new task");
            _ = Assert.Throws<DomainInvariantViolationException>(() => list.AddEntry(newEntry));
        }

        [Test]
        public void AddEntry_GivenNonPremiumOwnerAndListWithEntriesBelowNonPremiumLimit_AddsPendingEntriesToList()
        {
            var list = TaskList.ForOwner(NonPremiumOwner, 1, "list", 0);

            for (var i = 1; i <= TaskList.NonPremiumUserTaskEntryCountLimit - 1; i += 1)
            {
                var entry = TaskListEntry.ForAddingToTaskList(list.Id, i, $"task {i}");
                list = list.AddEntry(entry);
            }

            var newEntry = TaskListEntry.ForAddingToTaskList(list.Id, TaskList.NonPremiumUserTaskEntryCountLimit, "new task");
            Assert.DoesNotThrow(() => list.AddEntry(newEntry));
        }

        [Test]
        public void MarkEntryAsDone_GivenValidEntryId_MarksEntryAsDone()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            var entry1 = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task 1");
            var entry2 = TaskListEntry.ForAddingToTaskList(list.Id, 2, "task 2");
            list = list.AddEntry(entry1).AddEntry(entry2);

            var updatedList = list.MarkEntryAsDone(entry2.Id);

            Assert.Contains(entry1, updatedList.Entries);
            Assert.Contains(entry2.MarkAsDone(), updatedList.Entries);
        }

        [Test]
        public void MarkEntryAsDone_GivenInvalidEntryId_ThrowsException()
        {
            var entry = TaskListEntry.ForAddingToTaskList(1, 1, "task 1");
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0).AddEntry(entry);

            _ = Assert.Throws<DomainInvariantViolationException>(() => list.MarkEntryAsDone(99));
        }
    }
}
