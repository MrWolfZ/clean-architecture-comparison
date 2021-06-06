using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Persistence
{
    public interface ITaskListRepository : IAggregateRepository<TaskList, TaskListId>
    {
        public Task<TaskListEntryId> GenerateEntryId();

        public Task<IReadOnlyCollection<TaskList>> GetAll();

        public Task<int> GetNumberOfTaskListsByOwner(UserId ownerId);

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();
    }
}
