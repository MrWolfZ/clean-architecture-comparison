using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.QueryHandling;
using CAC.Core.Application.QueryHandling.Behaviors;

namespace CAC.CQS.Decorator.Application.TaskLists.GetAllTaskLists
{
    public sealed class GetAllTaskListsQueryHandler : IQueryHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        [QueryLoggingBehavior]
        [QueryValidationBehavior]
        public async Task<GetAllTaskListsQueryResponse> ExecuteQuery(GetAllTaskListsQuery query, CancellationToken cancellationToken)
        {
            var lists = await taskListRepository.GetAll();
            return GetAllTaskListsQueryResponse.FromTaskLists(lists);
        }
    }
}
