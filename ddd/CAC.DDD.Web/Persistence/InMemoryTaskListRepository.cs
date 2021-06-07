using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using CAC.Core.Infrastructure.Persistence;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Persistence
{
    internal sealed class InMemoryTaskListRepository : InMemoryAggregateRepository<TaskList, TaskListId>, ITaskListRepository
    {
        private long entryIdCounter;

        public InMemoryTaskListRepository(IDomainEventPublisher domainEventPublisher)
            : base(domainEventPublisher)
        {
        }

        public override async Task<TaskList> Upsert(TaskList taskList)
        {
            var all = await GetAll();
            if (all.Any(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId))
            {
                throw new UniquenessConstraintViolationException(taskList.Id, nameof(TaskList.Name), $"a task list with name '{taskList.Name}' already exists");
            }

            taskList = await base.Upsert(taskList);
            all = await GetAll();

            _ = Interlocked.Exchange(ref entryIdCounter, all.SelectMany(l => l.Entries).Select(e => e.Id.NumericValue).Concat(new[] { 0L }).Max());
            return taskList;
        }

        public Task<TaskListEntryId> GenerateEntryId() => Task.FromResult(TaskListEntryId.Of(Interlocked.Increment(ref entryIdCounter)));

        public new Task<IReadOnlyCollection<TaskList>> GetAll() => base.GetAll();

        public async Task<int> GetNumberOfTaskListsByOwner(UserId ownerId)
        {
            var all = await GetAll();
            return all.Count(l => l.OwnerId == ownerId);
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries()
        {
            var all = await GetAll();
            return all.Where(l => l.Entries.Any(i => !i.IsDone)).ToList();
        }

        protected override TaskListId CreateId(long numericId) => TaskListId.Of(numericId);
    }
}
