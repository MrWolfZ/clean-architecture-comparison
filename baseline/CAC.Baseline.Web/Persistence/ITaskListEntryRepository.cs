using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    public interface ITaskListEntryRepository
    {
        public Task<long> GenerateId();

        public Task Store(TaskListEntry entry);

        public Task<bool> MarkEntryAsDone(long entryId);
        
        public Task<TaskListEntry?> GetById(long id);

        public Task<IReadOnlyCollection<TaskListEntry>> GetEntriesForTaskList(long taskListId);

        public Task<IReadOnlyDictionary<long, IReadOnlyCollection<TaskListEntry>>> GetEntriesForTaskLists(IReadOnlyCollection<long> taskListIds);

        public Task<int> GetNumberOfEntriesForTaskList(long taskListId);

        public Task<IReadOnlyCollection<long>> GetIdsOfAllTaskListsWithPendingEntries();
    }
}
