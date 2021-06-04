using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Domain.TaskLists;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListService
    {
        Task<TaskList?> CreateNewTaskList(long ownerId, string name);
        
        Task<TaskList?> AddTaskToList(long taskListId, string taskDescription);
        
        Task<TaskList?> MarkTaskAsDone(long taskListId, int entryIdx);
        
        Task<IReadOnlyCollection<TaskList>> GetAll();
        
        Task<TaskList?> GetById(long taskListId);
        
        Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();
        
        Task<bool> DeleteById(long taskListId);
    }
}
