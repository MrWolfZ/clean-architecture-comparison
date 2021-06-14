using CAC.CQS.Decorator.Domain.TaskListAggregate;

namespace CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommandResponse(TaskListId Id);
}