using System;
using System.Threading.Tasks;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListReminderService
    {
        Task SendTaskListRemindersDueBefore(DateTimeOffset dueBefore);
    }
}
