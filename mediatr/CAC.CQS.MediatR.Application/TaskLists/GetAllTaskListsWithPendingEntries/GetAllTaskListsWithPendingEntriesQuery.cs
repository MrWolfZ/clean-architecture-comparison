using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetAllTaskListsWithPendingEntries
{
    public sealed record GetAllTaskListsWithPendingEntriesQuery : IRequest<GetAllTaskListsWithPendingEntriesQueryResponse>;
}
