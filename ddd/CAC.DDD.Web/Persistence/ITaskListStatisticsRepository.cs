using System.Threading.Tasks;
using CAC.DDD.Web.Domain;

namespace CAC.DDD.Web.Persistence
{
    public interface ITaskListStatisticsRepository
    {
        public Task Upsert(TaskListStatistics statistics);

        public Task<TaskListStatistics> Get();
    }
}
