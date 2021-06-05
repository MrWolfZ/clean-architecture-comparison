using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    internal sealed class InMemoryTaskListRepository : ITaskListRepository
    {
        private readonly ConcurrentDictionary<long, TaskList> listsById = new ConcurrentDictionary<long, TaskList>();
        private long idCounter;

        public Task<long> GenerateId() => Task.FromResult(Interlocked.Increment(ref idCounter));

        public Task Store(TaskList taskList)
        {
            if (listsById.ContainsKey(taskList.Id))
            {
                throw new ArgumentException($"task list '{taskList.Id}' already exists");
            }
            
            if (listsById.Values.Any(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId))
            {
                throw new ArgumentException($"a task list with name '{taskList.Name}' already exists");
            }

            listsById.AddOrUpdate(taskList.Id, _ => taskList, (_, _) => taskList);
            Interlocked.Exchange(ref idCounter, taskList.Id);
            return Task.CompletedTask;
        }

        public Task<bool> DeleteById(long id) => Task.FromResult(listsById.Remove(id, out _));

        public Task<bool> Exists(long id) => Task.FromResult(listsById.ContainsKey(id));

        public Task<TaskList?> GetById(long id)
        {
            var result = listsById.TryGetValue(id, out var taskList) ? taskList : null;
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<TaskList>> GetByIds(IReadOnlyCollection<long> ids)
        {
            var result = new List<TaskList>();

            foreach (var id in ids)
            {
                if (listsById.TryGetValue(id, out var taskList))
                {
                    result.Add(taskList);
                }
            }

            return Task.FromResult(result as IReadOnlyCollection<TaskList>);
        }

        public Task<IReadOnlyCollection<TaskList>> GetAll() => Task.FromResult<IReadOnlyCollection<TaskList>>(listsById.Values.ToList());

        public async Task<long?> GetOwnerId(long id) => (await GetById(id))?.OwnerId;

        public Task<int> GetNumberOfTaskListsByOwner(long ownerId) => Task.FromResult(listsById.Values.Count(l => l.OwnerId == ownerId));
    }
}
