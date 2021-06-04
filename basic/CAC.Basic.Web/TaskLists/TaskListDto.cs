using System.Collections.Generic;
using System.Linq;
using CAC.Basic.Domain.TaskLists;

namespace CAC.Basic.Web.TaskLists
{
    public sealed record TaskListDto(long Id, string Name, IReadOnlyCollection<TaskListEntryDto> Entries)
    {
        public static TaskListDto FromTaskList(TaskList taskList)
        {
            return new TaskListDto(taskList.Id, taskList.Name, taskList.Entries.Select(TaskListEntryDto.FromTaskListEntry).ToList());
        }
    }

    public sealed record TaskListEntryDto(string Description, bool IsDone)
    {
        public static TaskListEntryDto FromTaskListEntry(TaskListEntry taskListEntry)
        {
            return new TaskListEntryDto(taskListEntry.Description, taskListEntry.IsDone);
        }
    }
}
