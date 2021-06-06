using CAC.Core.Domain.Exceptions;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Domain.TaskListAggregate
{
    public sealed class TaskListTests
    {
        private static readonly User PremiumOwner = User.New(1, "premium", true);
        private static readonly User NonPremiumOwner = User.New(2, "non-premium", false);

        [Test]
        public void New_GivenValidName_CreatesList()
        {
            var id = TaskListId.Of(1);
            const string name = "list";
            var list = TaskList.New(id, PremiumOwner, name, 0);
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
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskList.New(1, PremiumOwner, name, 0));
        }

        [Test]
        public void New_GivenPremiumOwnerWithExistingList_CreatesList()
        {
            Assert.DoesNotThrow(() => TaskList.New(1, PremiumOwner, "list", 1));
        }

        [Test]
        public void New_GivenNonPremiumOwnerWithExistingList_ThrowsException()
        {
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskList.New(1, NonPremiumOwner, "list", 1));
        }

        [Test]
        public void AddEntry_GivenValidDescription_AddsPendingEntryToList()
        {
            var list = TaskList.New(1, PremiumOwner, "list", 0);
            const string description = "task";

            var entryId = TaskListEntryId.Of(1);
            var updatedList = list.AddEntry(entryId, description);

            Assert.Contains(TaskListEntry.New(list.Id, entryId, description, false), updatedList.Entries);
        }

        [Test]
        public void AddEntry_GivenMultipleValidDescriptions_AddsPendingEntriesToList()
        {
            var list = TaskList.New(1, PremiumOwner, "list", 0);
            const string description1 = "task 1";
            const string description2 = "task 2";

            var entryId1 = TaskListEntryId.Of(1);
            var entryId2 = TaskListEntryId.Of(2);
            var updatedList = list.AddEntry(entryId1, description1);
            updatedList = updatedList.AddEntry(entryId2, description2);

            Assert.AreEqual(2, updatedList.Entries.Count);
            Assert.Contains(TaskListEntry.New(list.Id, entryId1, description1, false), updatedList.Entries);
            Assert.Contains(TaskListEntry.New(list.Id, entryId2, description2, false), updatedList.Entries);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void AddEntry_GivenInvalidDescription_ThrowsException(string description)
        {
            var list = TaskList.New(1, PremiumOwner, "list", 0);

            _ = Assert.Throws<DomainInvariantViolationException>(() => list.AddEntry(1, description));
        }

        [Test]
        public void MarkEntryAsDone_GivenValidEntryId_MarksEntryAsDone()
        {
            const string description1 = "task 1";
            const string description2 = "task 2";
            var list = TaskList.New(1, PremiumOwner, "list", 0);

            var entryId1 = TaskListEntryId.Of(1);
            var entryId2 = TaskListEntryId.Of(2);
            list = list.AddEntry(entryId1, description1);
            list = list.AddEntry(entryId2, description2);

            var updatedList = list.MarkEntryAsDone(entryId2);

            Assert.Contains(TaskListEntry.New(list.Id, entryId1, description1, false), updatedList.Entries);
            Assert.Contains(TaskListEntry.New(list.Id, entryId2, description2, true), updatedList.Entries);
        }

        [Test]
        public void MarkEntryAsDone_GivenInvalidEntryId_ThrowsException()
        {
            var list = TaskList.New(1, PremiumOwner, "list", 0).AddEntry(1, "task");

            _ = Assert.Throws<DomainInvariantViolationException>(() => list.MarkEntryAsDone(99));
        }
    }
}
