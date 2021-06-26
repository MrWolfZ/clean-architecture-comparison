using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.QueryHandling;

namespace CAC.CQS.Application.TaskLists.GetAllTaskLists
{
    public sealed class GetAllTaskListsQueryHandler : IQueryHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<GetAllTaskListsQueryResponse> ExecuteQuery(GetAllTaskListsQuery query, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(query, new(query), true);

            var lists = await taskListRepository.GetAll();
            return GetAllTaskListsQueryResponse.FromTaskLists(lists);
        }
    }
}
