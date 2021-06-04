using System.Threading.Tasks;
using CAC.Basic.Domain.TaskLists;

namespace CAC.Basic.Application.TaskLists
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
