using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain.Exceptions;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed class GetTaskListByIdQueryHandler : IRequestHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetTaskListByIdQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        public async Task<GetTaskListByIdQueryResponse> Handle(GetTaskListByIdQuery query, CancellationToken cancellationToken)
        {
            var taskList = await taskListRepository.GetById(query.TaskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(query.TaskListId, $"task list {query.TaskListId} does not exist");
            }

            return GetTaskListByIdQueryResponse.FromTaskList(taskList);
        }
    }
}
