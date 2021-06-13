using CAC.CQS.Domain.TaskListAggregate;

namespace CAC.CQS.Application.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommandResponse(TaskListId Id);
}