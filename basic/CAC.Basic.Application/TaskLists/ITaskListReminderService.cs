using System.Threading.Tasks;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListReminderService
    {
        Task SendTaskListReminders();
    }
}
