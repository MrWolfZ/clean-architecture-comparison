using CAC.CQS.MediatR.Domain.TaskListAggregate;

namespace CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommandResponse(TaskListId Id);
}