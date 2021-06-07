using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain.Exceptions;
using CAC.Core.Infrastructure.Persistence;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Domain.UserAggregate;
using Microsoft.Extensions.Options;

namespace CAC.DDD.Web.Persistence
{
    internal sealed class FileSystemTaskListRepository : FileSystemAggregateRepository<TaskList, TaskListId, FileSystemTaskListRepository.TaskListPo>, ITaskListRepository
    {
        public FileSystemTaskListRepository(IOptions<FileSystemStoragePersistenceOptions> options, IDomainEventPublisher domainEventPublisher)
            : base(options, domainEventPublisher)
        {
        }

        public async Task<TaskListEntryId> GenerateEntryId() => await GenerateNumericIdForType<TaskListEntryId>();

        public override async Task Upsert(TaskList taskList)
        {
            if (taskList.IsDeleted)
            {
                await base.Upsert(taskList);
                return;
            }

            var all = await GetAll();

            if (all.Any(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId))
            {
                throw new UniquenessConstraintViolationException(taskList.Id, nameof(TaskList.Name), $"a task list with name '{taskList.Name}' already exists");
            }

            await base.Upsert(taskList);
        }

        public new async Task<IReadOnlyCollection<TaskList>> GetAll() => await base.GetAll();

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

        protected override TaskListPo ToPersistenceObject(TaskList aggregate)
        {
            var entryPos = aggregate.Entries.Select(e => new TaskListEntryPo(e.Id, e.Description, e.IsDone)).ToList();
            return new TaskListPo(aggregate.Id, aggregate.OwnerId, aggregate.Name, entryPos);
        }

        protected override TaskList FromPersistenceObject(TaskListPo persistenceObject)
        {
            var entries = persistenceObject.Entries.Select(e => TaskListEntry.New(persistenceObject.Id, e.Id, e.Description, e.IsDone)).ToValueList();
            return TaskList.New(persistenceObject.Id, persistenceObject.OwnerId, persistenceObject.Name, entries);
        }

        // persistence objects

        public sealed record TaskListPo(TaskListId Id, UserId OwnerId, string Name, IList<TaskListEntryPo> Entries);

        public sealed record TaskListEntryPo(TaskListEntryId Id, string Description, bool IsDone);
    }
}
