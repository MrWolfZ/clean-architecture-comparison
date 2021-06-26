using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.QueryHandling;
using CAC.Core.Application.QueryHandling.Behaviors;
using CAC.Core.Domain.Exceptions;

namespace CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery
{
    public sealed class GetTaskListByIdQueryHandler : IQueryHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse>
    {
        private readonly ITaskListRepository taskListRepository;

        public GetTaskListByIdQueryHandler(ITaskListRepository taskListRepository)
        {
            this.taskListRepository = taskListRepository;
        }

        [QueryLoggingBehavior]
        [QueryValidationBehavior]
        public async Task<GetTaskListByIdQueryResponse> ExecuteQuery(GetTaskListByIdQuery query, CancellationToken cancellationToken)
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
