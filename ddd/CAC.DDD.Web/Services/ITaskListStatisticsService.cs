using System.Threading.Tasks;
using CAC.DDD.Web.Domain;
using CAC.DDD.Web.Domain.TaskListAggregate;

namespace CAC.DDD.Web.Services
{
    public interface ITaskListStatisticsService
    {
        Task OnTaskListCreated(TaskList taskList);

        Task OnTaskAddedToList(TaskList taskList, TaskListEntryId taskListEntryId);

        Task OnTaskMarkedAsDone(TaskList taskList, TaskListEntryId taskListEntryId);

        Task OnTaskListDeleted(TaskListId taskListId);

        Task<TaskListStatistics> GetStatistics();
    }
}
