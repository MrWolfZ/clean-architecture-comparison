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
    internal sealed class FileSystemTaskListRepository : ITaskListRepository
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        private readonly string baseDir;

        public FileSystemTaskListRepository(IOptions<FileSystemStoragePersistenceOptions> options)
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

        public async Task Upsert(TaskList taskList)
        {
            var filePath = GetTaskListsFilePath();
            var all = await GetAll();

            var newLists = new List<TaskList>(all);

            if (newLists.Find(l => l.Id != taskList.Id && l.Name == taskList.Name && l.OwnerId == taskList.OwnerId) != null)
            {
                throw new ArgumentException($"a task list with name '{taskList.Name}' already exists");
            }

            var idx = newLists.FindIndex(l => l.Id == taskList.Id);

            if (idx >= 0)
            {
                newLists.RemoveAt(idx);
            }
            else
            {
                idx = newLists.Count;
            }

            newLists.Insert(idx, taskList);

            EnsureStorageDirExists();
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(newLists, SerializerOptions));
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAll()
        {
            var filePath = GetTaskListsFilePath();

            if (!File.Exists(filePath))
            {
                return Array.Empty<TaskList>();
            }

            var fileContent = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<TaskList>>(fileContent, SerializerOptions)!;
        }

        public async Task<int> GetNumberOfTaskListsByOwner(long ownerId)
        {
            var all = await GetAll();
            return all.Count(l => l.OwnerId == ownerId);
        }

        public async Task<TaskList?> GetById(long id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(l => l.Id == id);
        }

        public async Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries()
        {
            var all = await GetAll();
            return all.Where(l => l.Entries.Any(i => !i.IsDone)).ToList();
        }

        public async Task<bool> DeleteById(long id)
        {
            var filePath = GetTaskListsFilePath();
            var all = await GetAll();

            var newLists = new List<TaskList>(all);

            if (newLists.All(l => l.Id != id))
            {
                return false;
            }

            var idx = newLists.FindIndex(l => l.Id == id);
            newLists.RemoveAt(idx);

            EnsureStorageDirExists();
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(newLists, SerializerOptions));
            return true;
        }

        private string GetTaskListsFilePath() => Path.Join(GetStorageDir(), "task-lists.json");

        private string GetStorageDir()
        {
            if (!Directory.Exists(baseDir))
            {
                throw new InvalidOperationException($"file system repository base directory does not exist: '{baseDir}'");
            }

            return Path.Join(baseDir, "task-lists");
        }

        private void EnsureStorageDirExists() => Directory.CreateDirectory(GetStorageDir());
    }
}
