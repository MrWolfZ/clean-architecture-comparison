using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Data
{
    public interface ITaskListRepository
    {
        public Task<long> GenerateId();

        public Task Upsert(TaskList taskList);

        public Task<IReadOnlyCollection<TaskList>> GetAll();

        public Task<TaskList?> GetById(long id);

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();

        public Task<bool> DeleteById(long id);
    }
}
