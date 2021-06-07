using System.Collections.Generic;
using System.Linq;
using CAC.Basic.Domain.TaskListAggregate;

namespace CAC.Basic.Web.TaskLists
{
    public sealed record TaskListDto(TaskListId Id, string Name, IList<TaskListEntryDto> Entries)
    {
        public static TaskListDto FromTaskListEntry(TaskList list) =>
            new(list.Id, list.Name, list.Entries.Select(TaskListEntryDto.FromTaskListEntry).ToList());
    }

    public sealed record TaskListEntryDto(TaskListEntryId Id, string Description, bool IsDone)
    {
        public static TaskListEntryDto FromTaskListEntry(TaskListEntry entry) => new(entry.Id, entry.Description, entry.IsDone);
    }
}
