using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Domain.TaskLists;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListRepository
    {
        public Task<long> GenerateId();

        public Task Upsert(TaskList taskList);

        public Task<IReadOnlyCollection<TaskList>> GetAll();

        public Task<int> GetNumberOfTaskListsByOwner(long ownerId);

        public Task<TaskList?> GetById(long id);

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();

        public Task<bool> DeleteById(long id);
    }
}
