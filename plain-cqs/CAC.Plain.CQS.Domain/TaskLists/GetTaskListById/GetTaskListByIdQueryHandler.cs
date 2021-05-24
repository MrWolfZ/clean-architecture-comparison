using System.Threading.Tasks;

namespace CAC.Plain.CQS.Domain.TaskLists.GetTaskListById
{
    public sealed class GetTaskListByIdQueryHandler : IQueryHandler<TaskListId, GetTaskListByIdQueryResponse?>
    {
        private readonly ITaskListRepository repository;

        public GetTaskListByIdQueryHandler(ITaskListRepository repository) => this.repository = repository;

        public async Task<GetTaskListByIdQueryResponse?> ExecuteQuery(TaskListId taskListId)
        {
            var taskList = await repository.GetById(taskListId);
            return taskList == null ? null : new GetTaskListByIdQueryResponse(taskList);
        }
    }
}
