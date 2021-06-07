using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    internal sealed class InMemoryTaskListStatisticsRepository : ITaskListStatisticsRepository
    {
        private readonly object lockObject = new();
        private TaskListStatistics storeStatistics = new();

        public Task<TaskListStatistics> Get() => Task.FromResult(storeStatistics);

        public Task Upsert(TaskListStatistics statistics)
        {
            lock (lockObject)
            {
                storeStatistics = statistics;
                return Task.CompletedTask;
            }
        }
    }
}
