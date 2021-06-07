using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain;
using CAC.Core.Infrastructure.Serialization;
using Microsoft.Extensions.Options;

namespace CAC.Core.Infrastructure.Persistence
{
    public abstract class FileSystemAggregateRepository<TAggregate, TId, TPersistenceObject> : IAggregateRepository<TAggregate, TId>
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId : EntityId<TAggregate>
    {
        private readonly string baseDir;
        private readonly IDomainEventPublisher domainEventPublisher;

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        }.AddCoreConverters();

        protected FileSystemAggregateRepository(IOptions<FileSystemStoragePersistenceOptions> options, IDomainEventPublisher domainEventPublisher)
        {
            this.domainEventPublisher = domainEventPublisher;
            baseDir = options.Value.BaseDir;

            if (string.IsNullOrWhiteSpace(baseDir))
            {
                throw new ArgumentException("base dir in persistence options must not be empty", nameof(options));
            }
        }

        public async Task<TId> GenerateId() => CreateId(await GenerateNumericIdForType<TId>());

        public virtual async Task Upsert(TAggregate aggregate)
        {
            if (aggregate.IsDeleted)
            {
                await DeleteById(aggregate.Id);
                await domainEventPublisher.Publish(aggregate.DomainEvents);
                return;
            }

            var all = await LoadAll();

            var newLists = new List<TAggregate>(all);

            var idx = newLists.FindIndex(l => l.Id == aggregate.Id);

            if (idx >= 0)
            {
                newLists.RemoveAt(idx);
            }
            else
            {
                idx = newLists.Count;
            }

            newLists.Insert(idx, aggregate);

            await StoreAll(newLists);
            
            await domainEventPublisher.Publish(aggregate.DomainEvents);
        }

        public async Task<TAggregate?> GetById(TId id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(l => l.Id == id);
        }

        protected abstract TId CreateId(long numericId);

        protected abstract TPersistenceObject ToPersistenceObject(TAggregate aggregate);

        protected abstract TAggregate FromPersistenceObject(TPersistenceObject persistenceObject);

        protected async Task<IReadOnlyCollection<TAggregate>> GetAll() => await LoadAll();

        protected async Task<long> GenerateNumericIdForType<T>()
        {
            var idsFilePath = Path.Join(GetStorageDir(), $"{typeof(T).Name}.json");
            var fileContent = File.Exists(idsFilePath) ? await File.ReadAllTextAsync(idsFilePath) : "[]";
            var ids = JsonSerializer.Deserialize<List<long>>(fileContent, serializerOptions)!;
            var newId = ids.Count + 1;
            ids.Add(newId);
            EnsureStorageDirExists();
            await File.WriteAllTextAsync(idsFilePath, JsonSerializer.Serialize(ids, serializerOptions));
            return newId;
        }

        private async Task DeleteById(TId id)
        {
            var all = await LoadAll();

            var newLists = new List<TAggregate>(all);

            if (newLists.All(l => l.Id != id))
            {
                return;
            }

            var idx = newLists.FindIndex(l => l.Id == id);
            newLists.RemoveAt(idx);

            await StoreAll(newLists);
        }

        private async Task StoreAll(IReadOnlyCollection<TAggregate> items)
        {
            EnsureStorageDirExists();
            var filePath = GetTaskListsFilePath();
            var persistenceObjects = items.Select(ToPersistenceObject).ToList();
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(persistenceObjects, serializerOptions));
        }

        private async Task<IReadOnlyCollection<TAggregate>> LoadAll()
        {
            var filePath = GetTaskListsFilePath();

            if (!File.Exists(filePath))
            {
                return Array.Empty<TAggregate>();
            }

            var fileContent = await File.ReadAllTextAsync(filePath);
            var persistenceObjects = JsonSerializer.Deserialize<List<TPersistenceObject>>(fileContent, serializerOptions)!;
            return persistenceObjects.Select(FromPersistenceObject).ToList();
        }

        private string GetTaskListsFilePath() => Path.Join(GetStorageDir(), $"{typeof(TAggregate).Name}.json");

        private string GetStorageDir()
        {
            if (!Directory.Exists(baseDir))
            {
                throw new InvalidOperationException($"file system repository base directory does not exist: '{baseDir}'");
            }

            return Path.Join(baseDir, typeof(TAggregate).Name);
        }

        private void EnsureStorageDirExists() => Directory.CreateDirectory(GetStorageDir());
    }
}
