using System;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using CAC.CQS.Decorator.Domain.UserAggregate;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.Domain.TaskListAggregate
{
    public sealed class TaskListTests
    {
        private static readonly User PremiumOwner = User.FromRawData(1, "premium", true);
        private static readonly User NonPremiumOwner = User.FromRawData(2, "non-premium", false);

        [Test]
        public void ForOwner_GivenValidName_CreatesList()
        {
            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var id = TaskListId.Of(1);
            const string name = "list";
            var list = TaskList.ForOwner(PremiumOwner, id, name, 0);
            Assert.AreEqual(id, list.Id);
            Assert.AreEqual(PremiumOwner.Id, list.OwnerId);
            Assert.AreEqual(name, list.Name);
            Assert.AreEqual(DateTimeOffset.UnixEpoch, list.LastChangedAt);
            Assert.IsNull(list.LastReminderSentAt);
            Assert.IsEmpty(list.Entries);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void ForOwner_GivenEmptyName_ThrowsException(string name)
        {
            _ = Assert.Throws<DomainInvariantViolationException>(() => TaskList.ForOwner(PremiumOwner, 1, name, 0));
        }

        [Test]
        public void ForOwner_GivenPremiumOwnerWithExistingList_CreatesList()
        {
            Assert.DoesNotThrow(() => TaskList.ForOwner(PremiumOwner, 1, "list", 1));
        }

        [Test]
        public void ForOwner_GivenNonPremiumOwnerWithExistingList_ThrowsException()
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
        public void AddEntry_UpdatesLastChangedAt()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task");

            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var updatedList = list.AddEntry(entry);

            Assert.AreEqual(DateTimeOffset.UnixEpoch, updatedList.LastChangedAt);
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

        [Test]
        public void MarkEntryAsDone_UpdatesLastChangedAt()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task");
            list = list.AddEntry(entry);

            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var updatedList = list.MarkEntryAsDone(entry.Id);

            Assert.AreEqual(DateTimeOffset.UnixEpoch, updatedList.LastChangedAt);
        }

        [Test]
        public void MarkAsDeleted_MarksListAsDeleted()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            var updatedList = list.MarkAsDeleted();

            Assert.IsTrue(updatedList.IsDeleted);
        }

        [Test]
        public void MarkAsDeleted_UpdatesLastChangedAt()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var updatedList = list.MarkAsDeleted();

            Assert.AreEqual(DateTimeOffset.UnixEpoch, updatedList.LastChangedAt);
        }

        [Test]
        public void WithReminderSentAt_SetsLastReminderSentAt()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            var updatedList = list.WithReminderSentAt(DateTimeOffset.UnixEpoch);

            Assert.AreEqual(DateTimeOffset.UnixEpoch, updatedList.LastReminderSentAt);
        }

        [Test]
        public void WithReminderSentAt_UpdatesLastChangedAt()
        {
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var updatedList = list.WithReminderSentAt(DateTimeOffset.UnixEpoch);

            Assert.AreEqual(DateTimeOffset.UnixEpoch, updatedList.LastChangedAt);
        }

        [Test]
        public void IsDueForReminder_GivenLastChangedLessThanThresholdAgo_ReturnsFalse()
        {
            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);

            using var d2 = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch.Add(TaskList.ReminderDueAfter).AddDays(-1));

            Assert.IsFalse(list.IsDueForReminder());
        }

        [Test]
        public void IsDueForReminder_GivenLastChangedMoreThanThresholdAgoAndListHasNoPendingEntries_ReturnsFalse()
        {
            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task");
            list = list.AddEntry(entry).MarkEntryAsDone(entry.Id);

            using var d2 = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch.Add(TaskList.ReminderDueAfter).AddDays(1));

            Assert.IsFalse(list.IsDueForReminder());
        }

        [Test]
        public void IsDueForReminder_GivenLastChangedMoreThanThresholdAgoAndLastReminderSentLessThanThresholdAgo_ReturnsFalse()
        {
            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task");
            list = list.AddEntry(entry).WithReminderSentAt(DateTimeOffset.UnixEpoch.Add(TaskList.ReminderDueAfter));

            using var d2 = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch.Add(TaskList.ReminderDueAfter).AddDays(1));

            Assert.IsFalse(list.IsDueForReminder());
        }

        [Test]
        public void IsDueForReminder_GivenLastChangedMoreThanThresholdAgoAndLastReminderSentAtMoreThanThresholdAgoAndListHasPendingEntries_ReturnsTrue()
        {
            using var d = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch);
            var list = TaskList.ForOwner(PremiumOwner, 1, "list", 0);
            var entry = TaskListEntry.ForAddingToTaskList(list.Id, 1, "task");
            list = list.AddEntry(entry).WithReminderSentAt(DateTimeOffset.UnixEpoch);

            using var d2 = SystemTime.WithCurrentTime(DateTimeOffset.UnixEpoch.Add(TaskList.ReminderDueAfter).AddDays(1));

            Assert.IsTrue(list.IsDueForReminder());
        }
    }
}