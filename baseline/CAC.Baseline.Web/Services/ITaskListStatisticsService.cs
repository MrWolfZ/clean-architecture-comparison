using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Services
{
    public interface ITaskListStatisticsService
    {
        Task OnTaskListCreated(TaskList taskList);
        
        Task OnTaskAddedToList(TaskList taskList, int taskListEntryIdx);
        
        Task OnTaskMarkedAsDone(TaskList taskList, int taskListEntryIdx);
        
        Task OnTaskListDeleted(long taskListId);

        Task<TaskListStatistics> GetStatistics();
    }
}
