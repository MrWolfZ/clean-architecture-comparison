using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain;
using CAC.Core.Domain.Exceptions;
using CAC.Core.Infrastructure.Persistence;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Domain.TaskListAggregate;
using CAC.CQS.Domain.UserAggregate;
using Microsoft.Extensions.Options;

namespace CAC.CQS.Infrastructure.TaskLists
{
    internal sealed class FileSystemTaskListRepository : FileSystemAggregateRepository<TaskList, TaskListId, FileSystemTaskListRepository.TaskListPo>, ITaskListRepository
    {
        public FileSystemTaskListRepository(IOptions<FileSystemStoragePersistenceOptions> options, IDomainEventPublisher domainEventPublisher)
            : base(options, domainEventPublisher)
        {
        }

        public async Task<TaskListEntryId> GenerateEntryId() => await GenerateNumericIdForType<TaskListEntryId>();

        public override async Task<TaskList> Upsert(TaskList taskList)
        {
            if (taskList.IsDeleted)
            {
                return await base.Upsert(taskList);
            }

            var all = await GetAll();

            if (all.Any(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId))
            {
                throw new UniquenessConstraintViolationException(taskList.Id, nameof(TaskList.Name), $"a task list with name '{taskList.Name}' already exists");
            }

            return await base.Upsert(taskList);
        }

        public new async Task<IReadOnlyCollection<TaskList>> GetAll() => await base.GetAll();

        public async Task<IReadOnlyCollection<TaskList>> GetAllByOwner(UserId ownerId)
        {
            var all = await GetAll();
            return all.Where(l => l.OwnerId == ownerId).ToList();
        }

        public async Task<int> GetNumberOfTaskListsByOwner(UserId ownerId)
        {
            var all = await GetAllByOwner(ownerId);
            return all.Count;
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries()
        {
            var all = await GetAll();
            return all.Where(l => l.Entries.Any(i => !i.IsDone)).ToList();
        }

        protected override TaskListId CreateId(long numericId) => TaskListId.Of(numericId);

        protected override TaskListPo ToPersistenceObject(TaskList list)
        {
            var entryPos = list.Entries.Select(e => new TaskListEntryPo(e.Id, e.Description, e.IsDone)).ToList();
            return new(list.Id, list.OwnerId, list.OwnerIsPremium, list.Name, entryPos, list.LastChangedAt.ToIsoString(), list.LastReminderSentAt?.ToIsoString());
        }

        protected override TaskList FromPersistenceObject(TaskListPo po)
        {
            var entries = po.Entries.Select(e => TaskListEntry.FromRawData(e.Id, e.Description, e.IsDone)).ToValueList();
            return TaskList.FromRawData(po.Id, po.OwnerId, po.OwnerIsPremium, po.Name, entries, po.LastChangedAt.FromIsoString(), po.LastReminderSentAt?.FromIsoString());
        }

        // persistence objects

        public sealed record TaskListPo(TaskListId Id, UserId OwnerId, bool OwnerIsPremium, string Name, IList<TaskListEntryPo> Entries, string LastChangedAt, string? LastReminderSentAt);

        public sealed record TaskListEntryPo(TaskListEntryId Id, string Description, bool IsDone);
    }
}