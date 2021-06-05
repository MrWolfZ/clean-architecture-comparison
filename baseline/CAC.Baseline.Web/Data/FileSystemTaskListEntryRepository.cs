using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Core.Infrastructure;
using Microsoft.Extensions.Options;

namespace CAC.Baseline.Web.Data
{
    internal sealed class FileSystemTaskListEntryRepository : ITaskListEntryRepository
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        private readonly string baseDir;

        public FileSystemTaskListEntryRepository(IOptions<FileSystemStoragePersistenceOptions> options)
        {
            baseDir = options.Value.BaseDir;

            if (string.IsNullOrWhiteSpace(baseDir))
            {
                throw new ArgumentException("base dir in persistence options must not be empty", nameof(options));
            }
        }

        public async Task<long> GenerateId()
        {
            var idsFilePath = Path.Join(GetStorageDir(), "ids.json");
            var fileContent = File.Exists(idsFilePath) ? await File.ReadAllTextAsync(idsFilePath) : "[]";
            var ids = JsonSerializer.Deserialize<List<long>>(fileContent, SerializerOptions)!;
            var newId = ids.Count + 1;
            ids.Add(newId);
            EnsureStorageDirExists();
            await File.WriteAllTextAsync(idsFilePath, JsonSerializer.Serialize(ids, SerializerOptions));
            return newId;
        }

        public async Task Store(TaskListEntry entry)
        {
            var filePath = GetTaskListEntriesFilePath();
            var all = await GetAll();

            var newLists = new List<TaskListEntry>(all);

            if (newLists.Any(l => l.Id == entry.Id))
            {
                throw new ArgumentException($"entry '{entry.Id}' already exists");
            }

            newLists.Add(entry);

            EnsureStorageDirExists();
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(newLists, SerializerOptions));
        }

        public async Task<bool> MarkEntryAsDone(long entryId)
        {
            var filePath = GetTaskListEntriesFilePath();
            var all = await GetAll();

            var newLists = new List<TaskListEntry>(all);

            if (newLists.All(l => l.Id != entryId))
            {
                return false;
            }

            newLists.Find(l => l.Id == entryId)!.IsDone = true;

            EnsureStorageDirExists();
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(newLists, SerializerOptions));
            return true;
        }

        public async Task<TaskListEntry?> GetById(long id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(l => l.Id == id);
        }

        public async Task<IReadOnlyCollection<TaskListEntry>> GetEntriesForTaskList(long taskListId)
        {
            var all = await GetAll();
            return all.Where(entry => entry.OwningTaskListId == taskListId).ToList();
        }

        public async Task<IReadOnlyDictionary<long, IReadOnlyCollection<TaskListEntry>>> GetEntriesForTaskLists(IReadOnlyCollection<long> taskListIds)
        {
            var all = await GetAll();
            var result = all.Where(e => taskListIds.Contains(e.OwningTaskListId))
                            .GroupBy(e => e.OwningTaskListId)
                            .ToDictionary(g => g.Key, g => g.ToList() as IReadOnlyCollection<TaskListEntry>);

            foreach (var id in taskListIds)
            {
                result.TryAdd(id, new List<TaskListEntry>());
            }

            return result;
        }

        public async Task<int> GetNumberOfEntriesForTaskList(long taskListId) => (await GetEntriesForTaskList(taskListId)).Count;

        public async Task<IReadOnlyCollection<long>> GetIdsOfAllTaskListsWithPendingEntries()
        {
            var all = await GetAll();
            var result = all.Where(e => !e.IsDone)
                            .Select(e => e.OwningTaskListId)
                            .Distinct()
                            .ToList();

            return result;
        }

        private async Task<IReadOnlyCollection<TaskListEntry>> GetAll()
        {
            var filePath = GetTaskListEntriesFilePath();

            if (!File.Exists(filePath))
            {
                return Array.Empty<TaskListEntry>();
            }

            var fileContent = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<TaskListEntry>>(fileContent, SerializerOptions)!;
        }

        private string GetTaskListEntriesFilePath()
        {
            return Path.Join(GetStorageDir(), "task-list-entries.json");
        }

        private string GetStorageDir()
        {
            if (!Directory.Exists(baseDir))
            {
                throw new InvalidOperationException($"file system repository base directory does not exist: '{baseDir}'");
            }

            return Path.Join(baseDir, "task-list-entries");
        }

        private void EnsureStorageDirExists()
        {
            Directory.CreateDirectory(GetStorageDir());
        }
    }
}
