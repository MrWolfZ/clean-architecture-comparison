using System.Threading.Tasks;

namespace CAC.CQS.Application.TaskLists
{
    public interface ITaskListStatisticsRepository
    {
        public Task Upsert(TaskListStatistics statistics);

        public Task<TaskListStatistics> Get();
    }
}