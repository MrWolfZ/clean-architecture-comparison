using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CAC.CQS.MediatR.Domain.TaskListAggregate;

// nesting DTOs for query responses is okay
#pragma warning disable CA1034

namespace CAC.CQS.MediatR.Application.TaskLists.GetAllTaskListsWithPendingEntries
{
    public sealed record GetAllTaskListsWithPendingEntriesQueryResponse(ValueList<GetAllTaskListsWithPendingEntriesQueryResponse.TaskListDto> TaskLists)
    {
        public static GetAllTaskListsWithPendingEntriesQueryResponse FromTaskLists(IReadOnlyCollection<TaskList> lists) =>
            new(lists.Select(TaskListDto.FromTaskList).OrderBy(dto => dto.Id).ToValueList());

        public sealed record TaskListDto(TaskListId Id, string Name, ValueList<TaskListEntryDto> Entries)
        {
            public static TaskListDto FromTaskList(TaskList list) =>
                new(list.Id, list.Name, list.Entries.Select(TaskListEntryDto.FromTaskListEntry).ToValueList());
        }

        public sealed record TaskListEntryDto(TaskListEntryId Id, string Description, bool IsDone)
        {
            public static TaskListEntryDto FromTaskListEntry(TaskListEntry entry) => new(entry.Id, entry.Description, entry.IsDone);
        }
    }
}
