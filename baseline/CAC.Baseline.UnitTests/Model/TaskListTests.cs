using System;
using System.Linq;
using CAC.Baseline.Web.Model;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace CAC.Baseline.UnitTests.Model
{
    public sealed class TaskListTests
    {
        private const long OwnerId = 1;
        
        [Test]
        public void New_GivenValidName_CreatesList()
        {
            const long id = 1;
            const string name = "test list";
            var list = new TaskList(id, OwnerId, name);
            Assert.AreEqual(id, list.Id);
            Assert.AreEqual(name, list.Name);
            Assert.IsEmpty(list.Entries);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_GivenInvalidId_ThrowsArgumentException(long id)
        {
            Assert.Throws<ArgumentException>(() => new TaskList(id, OwnerId, "test list"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_GivenInvalidOwnerId_ThrowsArgumentException(long ownerId)
        {
            Assert.Throws<ArgumentException>(() => new TaskList(1, ownerId, "test list"));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Constructor_GivenEmptyName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => new TaskList(1, OwnerId, name));
        }

        [Test]
        public void AddEntry_GivenValidDescription_AddsPendingEntryToList()
        {
            var list = new TaskList(1, OwnerId, "test list");
            const string description = "task";

            list.AddEntry(description);

            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description, false)));
        }

        [Test]
        public void AddEntry_GivenMultipleValidDescriptions_AddsPendingEntriesToList()
        {
            var list = new TaskList(1, OwnerId, "test list");
            const string description1 = "task 1";
            const string description2 = "task 2";

            list.AddEntry(description1);
            list.AddEntry(description2);

            Assert.AreEqual(2, list.Entries.Count);
            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description1, false)));
            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description2, false)));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void AddEntry_GivenInvalidDescription_ThrowsArgumentException(string description)
        {
            var list = new TaskList(1, OwnerId, "test list");

            Assert.Throws<ArgumentException>(() => list.AddEntry(description));
        }

        [Test]
        public void MarkEntryAsDone_GivenValidEntryIndex_MarksEntryAsDone()
        {
            const string description1 = "task 1";
            const string description2 = "task 2";
            var list = new TaskList(1, OwnerId, "test list");
            list.AddEntry(description1);
            list.AddEntry(description2);

            list.MarkEntryAsDone(1);

            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description1, false)));
            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description2, true)));
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void MarkEntryAsDone_GivenInvalidEntryIndex_ThrowsArgumentException(int entryIdx)
        {
            var list = new TaskList(1, OwnerId, "test list");
            list.AddEntry("task 1");
            list.AddEntry("task 2");

            Assert.Throws<ArgumentException>(() => list.MarkEntryAsDone(entryIdx));
        }
    }
}
