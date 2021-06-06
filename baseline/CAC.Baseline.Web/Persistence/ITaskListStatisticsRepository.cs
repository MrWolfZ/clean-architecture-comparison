using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    public interface ITaskListStatisticsRepository
    {
        public Task Upsert(TaskListStatistics statistics);

        public Task<TaskListStatistics> Get();
    }
}
