using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;

namespace CAC.CQS.Decorator.Application.TaskLists.GetAllTaskListsWithPendingEntries
{
    public sealed class GetAllTaskListsWithPendingEntriesQueryHandler : IQueryHandler<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsWithPendingEntriesQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        [LogQuery]
        [ValidateQuery]
        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> ExecuteQuery(GetAllTaskListsWithPendingEntriesQuery query, CancellationToken cancellationToken)
        {
            var lists = await taskListRepository.GetAllWithPendingEntries();
            return GetAllTaskListsWithPendingEntriesQueryResponse.FromTaskLists(lists);
        }
    }
}
