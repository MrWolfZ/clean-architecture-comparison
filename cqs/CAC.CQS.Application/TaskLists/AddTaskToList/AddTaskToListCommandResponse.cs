using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommandResponse(TaskListEntryId EntryId);
}
