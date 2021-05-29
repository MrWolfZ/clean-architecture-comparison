using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain;
using CAC.CQS.Domain.TaskLists;

namespace CAC.CQS.Infrastructure.TaskLists
{
    internal sealed class InMemoryTaskListRepository : ITaskListRepository
    {
        private readonly ConcurrentDictionary<EntityId, TaskList> listsById = new ConcurrentDictionary<EntityId, TaskList>();
        private int idCounter;

        public Task Upsert(TaskList taskList)
        {
            if (listsById.Values.Any(l => l.Id != taskList.Id && l.Name == taskList.Name))
            {
                throw new UniquenessConstraintDomainValidationException(taskList.Id, nameof(TaskList.Name), taskList.Name);
            }

            listsById.AddOrUpdate(taskList.Id, _ => taskList, (_, _) => taskList);
            return Task.CompletedTask;
        }

        public Task<TaskListId> GenerateId() => Task.FromResult(TaskListId.Of(Interlocked.Increment(ref idCounter)));

        public Task<TaskList?> GetById(TaskListId id)
        {
            var result = listsById.TryGetValue(id, out var taskList) ? taskList : null;
            return Task.FromResult(result);
        }
    }
}
