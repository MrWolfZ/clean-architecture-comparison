using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetAllTaskListsWithPendingEntries
{
    public sealed class GetAllTaskListsWithPendingEntriesQueryHandler : IRequestHandler<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsWithPendingEntriesQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> Handle(GetAllTaskListsWithPendingEntriesQuery query, CancellationToken cancellationToken)
        {
            var lists = await taskListRepository.GetAllWithPendingEntries();
            return GetAllTaskListsWithPendingEntriesQueryResponse.FromTaskLists(lists);
        }
    }
}
