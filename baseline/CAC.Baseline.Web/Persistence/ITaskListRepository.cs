using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    public interface ITaskListRepository
    {
        public Task<long> GenerateId();

        public Task Store(TaskList taskList);

        public Task<bool> DeleteById(long id);

        public Task<bool> Exists(long id);

        public Task<TaskList?> GetById(long id);

        public Task<IReadOnlyCollection<TaskList>> GetByIds(IReadOnlyCollection<long> ids);

        public Task<IReadOnlyCollection<TaskList>> GetAll();

        public Task<long?> GetOwnerId(long id);

        public Task<int> GetNumberOfTaskListsByOwner(long ownerId);
    }
}
