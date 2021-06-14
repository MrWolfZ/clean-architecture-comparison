using System.Collections.Immutable;
using System.Linq;
using CAC.CQS.MediatR.Domain.TaskListAggregate;

// nesting DTOs for query responses is okay
#pragma warning disable CA1034

namespace CAC.CQS.MediatR.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed record GetTaskListByIdQueryResponse(TaskListId Id, string Name, ValueList<GetTaskListByIdQueryResponse.TaskListEntryDto> Entries)
    {
        public static GetTaskListByIdQueryResponse FromTaskList(TaskList list) =>
            new(list.Id, list.Name, list.Entries.Select(TaskListEntryDto.FromTaskListEntry).ToValueList());

        public sealed record TaskListEntryDto(TaskListEntryId Id, string Description, bool IsDone)
        {
            public static TaskListEntryDto FromTaskListEntry(TaskListEntry entry) => new(entry.Id, entry.Description, entry.IsDone);
        }
    }
}
