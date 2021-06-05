using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Services
{
    public interface ITaskListStatisticsService
    {
        Task OnTaskListCreated(TaskList taskList);
        
        Task OnTaskAddedToList(TaskListEntry taskListEntry);
        
        Task OnTaskMarkedAsDone(long taskListEntryId);
        
        Task OnTaskListDeleted(long taskListId);

        Task<TaskListStatistics> GetStatistics();
    }
}
