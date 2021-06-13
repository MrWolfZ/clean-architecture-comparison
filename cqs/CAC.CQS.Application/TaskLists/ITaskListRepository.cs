using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;

namespace CAC.CQS.Application.TaskLists
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