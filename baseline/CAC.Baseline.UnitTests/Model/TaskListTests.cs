using System;
using System.Linq;
using CAC.Baseline.Web.Model;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace CAC.Baseline.UnitTests.Model
{
    public sealed class TaskListTests
    {
        [Test]
        public void New_GivenValidName_CreatesList()
        {
            const long id = 1;
            const string name = "test list";
            var list = new TaskList(id, name);
            Assert.AreEqual(id, list.Id);
            Assert.AreEqual(name, list.Name);
            Assert.IsEmpty(list.Entries);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_GivenInvalidId_ThrowsArgumentException(long id)
        {
            Assert.Throws<ArgumentException>(() => new TaskList(id, "test list"));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Constructor_GivenEmptyName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => new TaskList(1, name));
        }

        [Test]
        public void Constructor_GivenNameWithTooManyCharacters_ThrowsArgumentException()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", TaskList.MaxTaskListNameLength + 1));
            Assert.Throws<ArgumentException>(() => new TaskList(1, name));
        }

        [Test]
        public void AddEntry_GivenValidDescription_AddsPendingEntryToList()
        {
            var list = new TaskList(1, "test list");
            const string description = "task";

            list.AddEntry(description);

            Assert.IsTrue(list.Entries.Contains(new TaskListEntry(description, false)));
        }

        [Test]
        public void AddEntry_GivenMultipleValidDescriptions_AddsPendingEntriesToList()
        {
            var list = new TaskList(1, "test list");
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
        public void AddEntry_GivenEmptyDescription_ThrowsArgumentException(string description)
        {
            var list = new TaskList(1, "test list");

            Assert.Throws<ArgumentException>(() => list.AddEntry(description));
        }

        [Test]
        public void MarkEntryAsDone_GivenValidEntryIndex_MarksEntryAsDone()
        {
            const string description1 = "task 1";
            const string description2 = "task 2";
            var list = new TaskList(1, "test list");
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
            var list = new TaskList(1, "test list");
            list.AddEntry("task 1");
            list.AddEntry("task 2");

            Assert.Throws<ArgumentException>(() => list.MarkEntryAsDone(entryIdx));
        }
    }
}
