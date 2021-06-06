using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Services
{
    public interface ITaskListNotificationService
    {
        Task OnTaskListCreated(TaskList taskList);
        
        Task OnTaskAddedToList(TaskListEntry taskListEntry);
        
        Task OnTaskMarkedAsDone(long taskListId, long taskListEntryId);
        
        Task OnTaskListDeleted(long taskListId);
    }
}
