using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetAllTaskLists
{
    public sealed class GetAllTaskListsQueryHandler : IRequestHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetAllTaskListsQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<GetAllTaskListsQueryResponse> Handle(GetAllTaskListsQuery query, CancellationToken cancellationToken)
        {
            var lists = await taskListRepository.GetAll();
            return GetAllTaskListsQueryResponse.FromTaskLists(lists);
        }
    }
}
