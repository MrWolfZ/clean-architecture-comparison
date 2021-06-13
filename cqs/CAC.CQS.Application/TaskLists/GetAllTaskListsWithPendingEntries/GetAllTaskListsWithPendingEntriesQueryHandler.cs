using System.Threading.Tasks;
using CAC.Core.Application;

namespace CAC.CQS.Application.TaskLists.GetAllTaskListsWithPendingEntries
{
    public sealed class GetAllTaskListsWithPendingEntriesQueryHandler : IQueryHandler<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsWithPendingEntriesQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> ExecuteQuery(GetAllTaskListsWithPendingEntriesQuery query)
        {
            var lists = await taskListRepository.GetAllWithPendingEntries();
            return GetAllTaskListsWithPendingEntriesQueryResponse.FromTaskLists(lists);
        }
    }
}
