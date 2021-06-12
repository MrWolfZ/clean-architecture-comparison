using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Basic.Domain.UserAggregate;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListService
    {
        Task<TaskList> CreateNewTaskList(UserId ownerId, string name);
        
        Task<(TaskList TaskList, TaskListEntryId EntryId)> AddTaskToList(TaskListId taskListId, string taskDescription);
        
        Task<TaskList> MarkTaskAsDone(TaskListId taskListId, TaskListEntryId entryId);
        
        Task<IReadOnlyCollection<TaskList>> GetAll();
        
        Task<TaskList> GetById(TaskListId taskListId);
        
        Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();
        
        Task<TaskList> DeleteById(TaskListId taskListId);
    }
}
