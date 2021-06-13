using System.Threading.Tasks;
using CAC.CQS.Application.TaskLists;

namespace CAC.CQS.Infrastructure.TaskLists
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