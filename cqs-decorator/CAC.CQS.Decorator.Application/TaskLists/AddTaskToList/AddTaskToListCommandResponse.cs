using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.AddTaskToList
{
    public sealed record AddTaskToListCommandResponse(TaskListEntryId EntryId);
}
