using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Basic.Domain.UserAggregate;
using CAC.Core.Application;

namespace CAC.Basic.Application.TaskLists
{
    public interface ITaskListRepository : IAggregateRepository<TaskList, TaskListId>
    {
        public Task<TaskListEntryId> GenerateEntryId();

        public Task<IReadOnlyCollection<TaskList>> GetAll();

        public Task<IReadOnlyCollection<TaskList>> GetAllByOwner(UserId ownerId);

        public Task<int> GetNumberOfTaskListsByOwner(UserId ownerId);

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries();
    }
}
