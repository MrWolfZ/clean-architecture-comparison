using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Data
{
    internal sealed class InMemoryTaskListRepository : ITaskListRepository
    {
        private readonly ConcurrentDictionary<long, TaskList> listsById = new ConcurrentDictionary<long, TaskList>();
        private long idCounter;

        public Task Upsert(TaskList taskList)
        {
            if (listsById.Values.Any(l => l.Id != taskList.Id && l.Name == taskList.Name))
            {
                throw new ArgumentException($"a task list with name '{taskList.Name}' already exists");
            }

            listsById.AddOrUpdate(taskList.Id, _ => taskList, (_, _) => taskList);
            return Task.CompletedTask;
        }

        public Task<long> GenerateId() => Task.FromResult(Interlocked.Increment(ref idCounter));

        public Task<IReadOnlyCollection<TaskList>> GetAll() => Task.FromResult<IReadOnlyCollection<TaskList>>(listsById.Values.ToList());

        public Task<TaskList?> GetById(long id)
        {
            var result = listsById.TryGetValue(id, out var taskList) ? taskList : null;
            return Task.FromResult(result);
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries()
        {
            var all = await GetAll();
            return all.Where(l => l.Entries.Any(i => !i.IsDone)).ToList();
        }

        public Task<bool> DeleteById(long id) => Task.FromResult(listsById.Remove(id, out _));
    }
}
