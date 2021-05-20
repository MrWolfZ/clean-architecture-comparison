using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Plain.Domain.TaskLists;
using NUnit.Framework;

namespace CAC.Plain.UnitTests.Infrastructure.TaskLists
{
    public abstract class TaskListRepositoryTests
    {
        protected abstract ITaskListRepository Testee { get; }
        
        [Test]
        public async Task GenerateId_ReturnsNewIdOnEachCall()
        {
            var generatedIds = new HashSet<TaskListId>();

            for (var i = 0; i < 100; i += 1)
            {
                var id = await Testee.GenerateId();
                Assert.IsFalse(generatedIds.Contains(id));
                generatedIds.Add(id);
            }
        }
        
        [Test]
        public async Task Upsert_GivenNonExistingTaskList_StoresTaskList()
        {
            var list = TaskList.New(1, "test").AddItem("task");
            await Testee.Upsert(list);
            
            Assert.AreEqual(list, await Testee.GetById(list.Id));
        }
        
        [Test]
        public async Task Upsert_GivenExistingTaskList_StoresTaskList()
        {
            var existingList = TaskList.New(1, "test");
            await Testee.Upsert(existingList);
            
            var list = TaskList.New(1, "test").AddItem("task");
            await Testee.Upsert(list);
            
            Assert.AreEqual(list, await Testee.GetById(list.Id));
        }
    }
}
