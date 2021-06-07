using System.Threading.Tasks;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListStatisticsRepository
    {
        public Task Upsert(TaskListStatistics statistics);

        public Task<TaskListStatistics> Get();
    }
}
