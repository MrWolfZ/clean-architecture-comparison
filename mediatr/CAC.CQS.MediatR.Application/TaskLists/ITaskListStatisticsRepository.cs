using System.Threading.Tasks;

namespace CAC.CQS.MediatR.Application.TaskLists
{
    public interface ITaskListStatisticsRepository
    {
        public Task Upsert(TaskListStatistics statistics);

        public Task<TaskListStatistics> Get();
    }
}