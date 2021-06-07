using System.Threading.Tasks;
using CAC.DDD.Web.Domain;

namespace CAC.DDD.Web.Persistence
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
