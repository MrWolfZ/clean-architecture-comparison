using CAC.CQS.MediatR.Domain.TaskListAggregate;

namespace CAC.CQS.MediatR.Application.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommandResponse(TaskListEntryId EntryId);
}
