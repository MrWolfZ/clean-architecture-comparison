using System.Collections.Immutable;
using System.Linq;
using CAC.Core.Domain;
using CAC.Plain.Domain.TaskLists;
using NUnit.Framework;

namespace CAC.Plain.UnitTests.Domain.TaskLists
{
    public sealed class TaskListTests
    {
        [Test]
        public void New_GivenValidName_CreatesList()
        {
            var id = TaskListId.Of(1);
            const string name = "test list";
            var list = TaskList.New(id, name);
            Assert.AreEqual(id, list.Id);
            Assert.AreEqual(name, list.Name);
            Assert.IsEmpty(list.Items);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void New_GivenEmptyName_ThrowsDomainValidationException(string name)
        {
            Assert.Throws<DomainValidationException>(() => TaskList.New(1, name));
        }

        [Test]
        public void New_GivenNameWithTooManyCharacters_ThrowsDomainValidationException()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", TaskList.MaxTaskListNameLength + 1));
            Assert.Throws<DomainValidationException>(() => TaskList.New(1, name));
        }
        
        [Test]
        public void AddItem_GivenValidDescription_AddsPendingItemToList()
        {
            var list = TaskList.New(1, "test list");
            const string description = "task";
            var expectedItems = list.Items.Add(TaskListItem.New(description, false));

            var updatedList = list.AddItem(description);
            
            Assert.AreEqual(list.Id, updatedList.Id);
            Assert.AreEqual(list.Name, updatedList.Name);
            Assert.AreEqual(expectedItems, updatedList.Items);
        }
        
        [Test]
        public void AddItem_GivenMultipleValidDescriptions_AddsPendingItemsToList()
        {
            var list = TaskList.New(1, "test list");
            const string description1 = "task 1";
            const string description2 = "task 2";
            var expectedItems = list.Items.Add(TaskListItem.New(description1, false)).Add(TaskListItem.New(description2, false));
            
            var updatedList = list.AddItem(description1).AddItem(description2);
            
            Assert.AreEqual(list.Id, updatedList.Id);
            Assert.AreEqual(list.Name, updatedList.Name);
            Assert.AreEqual(expectedItems, updatedList.Items);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void AddItem_GivenEmptyDescription_ThrowsDomainValidationException(string description)
        {
            var list = TaskList.New(1, "test list");
            
            Assert.Throws<DomainValidationException>(() => list.AddItem(description));
        }
        
        [Test]
        public void MarkItemAsDone_GivenValidItemIndex_MarksItemAsDone()
        {
            const string description1 = "task 1";
            const string description2 = "task 2";
            var list = TaskList.New(1, "test list").AddItem(description1).AddItem(description2);
            var expectedItems = ValueList<TaskListItem>.Empty.Add(TaskListItem.New(description1, false)).Add(TaskListItem.New(description2, true));

            var updatedList = list.MarkItemAsDone(1);
            
            Assert.AreEqual(updatedList.Id, list.Id);
            Assert.AreEqual(updatedList.Name, list.Name);
            Assert.AreEqual(expectedItems, updatedList.Items);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void MarkItemAsDone_GivenInvalidItemIndex_ThrowsDomainValidationException(int itemIdx)
        {
            var list = TaskList.New(1, "test list").AddItem("task 1").AddItem("task 2");
            
            Assert.Throws<DomainValidationException>(() => list.MarkItemAsDone(itemIdx));
        }
    }
}
