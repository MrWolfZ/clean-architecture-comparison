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
            Assert.IsEmpty(list.Items);
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
        public void AddItem_GivenValidDescription_AddsPendingItemToList()
        {
            var list = new TaskList(1, "test list");
            const string description = "task";

            list.AddItem(description);

            Assert.IsTrue(list.Items.Contains(new TaskListItem(description, false)));
        }

        [Test]
        public void AddItem_GivenMultipleValidDescriptions_AddsPendingItemsToList()
        {
            var list = new TaskList(1, "test list");
            const string description1 = "task 1";
            const string description2 = "task 2";

            list.AddItem(description1);
            list.AddItem(description2);

            Assert.AreEqual(2, list.Items.Count);
            Assert.IsTrue(list.Items.Contains(new TaskListItem(description1, false)));
            Assert.IsTrue(list.Items.Contains(new TaskListItem(description2, false)));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void AddItem_GivenEmptyDescription_ThrowsArgumentException(string description)
        {
            var list = new TaskList(1, "test list");

            Assert.Throws<ArgumentException>(() => list.AddItem(description));
        }

        [Test]
        public void MarkItemAsDone_GivenValidItemIndex_MarksItemAsDone()
        {
            const string description1 = "task 1";
            const string description2 = "task 2";
            var list = new TaskList(1, "test list");
            list.AddItem(description1);
            list.AddItem(description2);

            list.MarkItemAsDone(1);

            Assert.IsTrue(list.Items.Contains(new TaskListItem(description1, false)));
            Assert.IsTrue(list.Items.Contains(new TaskListItem(description2, true)));
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void MarkItemAsDone_GivenInvalidItemIndex_ThrowsArgumentException(int itemIdx)
        {
            var list = new TaskList(1, "test list");
            list.AddItem("task 1");
            list.AddItem("task 2");

            Assert.Throws<ArgumentException>(() => list.MarkItemAsDone(itemIdx));
        }
    }
}
