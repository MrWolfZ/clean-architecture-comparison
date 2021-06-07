using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    internal sealed class InMemoryTaskListEntryRepository : ITaskListEntryRepository
    {
        private readonly ConcurrentDictionary<long, TaskListEntry> entriesById = new();
        private long idCounter;

        public Task<long> GenerateId()
        {
            return Task.FromResult(Interlocked.Increment(ref idCounter));
        }

        public Task Store(TaskListEntry entry)
        {
            if (entriesById.ContainsKey(entry.Id))
            {
                throw new ArgumentException($"entry '{entry.Id}' already exists");
            }

            _ = entriesById.AddOrUpdate(entry.Id, _ => entry, (_, _) => entry);
            _ = Interlocked.Exchange(ref idCounter, entry.Id);
            return Task.CompletedTask;
        }

        public Task<bool> MarkEntryAsDone(long entryId)
        {
            if (entriesById.TryGetValue(entryId, out var entry))
            {
                entry.IsDone = true;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<TaskListEntry?> GetById(long id)
        {
            var result = entriesById.TryGetValue(id, out var entry) ? entry : null;
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<TaskListEntry>> GetEntriesForTaskList(long taskListId)
        {
            var result = entriesById.Values.Where(entry => entry.OwningTaskListId == taskListId).ToList();
            return Task.FromResult(result as IReadOnlyCollection<TaskListEntry>);
        }

        public Task<IReadOnlyDictionary<long, IReadOnlyCollection<TaskListEntry>>> GetEntriesForTaskLists(IReadOnlyCollection<long> taskListIds)
        {
            var result = entriesById.Values
                                    .Where(e => taskListIds.Contains(e.OwningTaskListId))
                                    .GroupBy(e => e.OwningTaskListId)
                                    .ToDictionary(g => g.Key, g => g.ToList() as IReadOnlyCollection<TaskListEntry>);

            foreach (var id in taskListIds)
            {
                _ = result.TryAdd(id, new List<TaskListEntry>());
            }

            return Task.FromResult(result as IReadOnlyDictionary<long, IReadOnlyCollection<TaskListEntry>>);
        }

        public async Task<int> GetNumberOfEntriesForTaskList(long taskListId) => (await GetEntriesForTaskList(taskListId)).Count;

        public Task<IReadOnlyCollection<long>> GetIdsOfAllTaskListsWithPendingEntries()
        {
            var result = entriesById.Values
                                    .Where(e => !e.IsDone)
                                    .Select(e => e.OwningTaskListId)
                                    .Distinct()
                                    .ToList();
            
            return Task.FromResult(result as IReadOnlyCollection<long>);
        }
    }
}
