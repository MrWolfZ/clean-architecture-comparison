using System.Threading.Tasks;

namespace CAC.Plain.CQS.Domain.TaskLists
{
    public interface ITaskListRepository
    {
        public Task<TaskListId> GenerateId();

        public Task Upsert(TaskList taskList);

        public Task<TaskList?> GetById(TaskListId id);
    }
}
