using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetAllTaskLists
{
    public sealed record GetAllTaskListsQuery : IRequest<GetAllTaskListsQueryResponse>;
}
