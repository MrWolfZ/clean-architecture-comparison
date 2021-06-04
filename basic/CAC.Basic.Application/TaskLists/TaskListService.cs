using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Application.Users;
using CAC.Basic.Domain.TaskLists;
using Microsoft.Extensions.Logging;

namespace CAC.Basic.Application.TaskLists
{
    internal sealed class TaskListService : ITaskListService
    {
        public const int MaxTaskListNameLength = 64;
        public const int MaxTaskDescriptionLength = 256;
        
        private const int NonPremiumUserTaskEntryCountLimit = 5;

        private readonly ILogger<TaskListService> logger;
        private readonly ITaskListStatisticsService statisticsService;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListService(ITaskListRepository taskListRepository,
                               IUserRepository userRepository,
                               ITaskListStatisticsService statisticsService,
                               ILogger<TaskListService> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
            this.statisticsService = statisticsService;
            this.userRepository = userRepository;
        }

        public async Task<TaskList?> CreateNewTaskList(long ownerId, string name)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            
            if (name.Length > MaxTaskListNameLength)
            {
                throw new ArgumentException($"task list name can be at most {MaxTaskListNameLength} characters long, but was {name.Length}", nameof(name));
            }
            
            var user = await userRepository.GetById(ownerId);

            if (user == null)
            {
                return null;
            }

            if (!user.IsPremium)
            {
                var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(ownerId);

                if (numberOfListsOwnedByOwner > 0)
                {
                    throw new ArgumentException($"non-premium user {ownerId} already owns a task list");
                }
            }

            var id = await taskListRepository.GenerateId();
            var taskList = new TaskList(id, ownerId, name);

            await taskListRepository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{TaskListId}' for owner '{OwnerId}'...", name, id, taskList.OwnerId);

            await statisticsService.OnTaskListCreated(taskList);

            return taskList;
        }

        public async Task<TaskList?> AddTaskToList(long taskListId, string taskDescription)
        {
            taskDescription = taskDescription ?? throw new ArgumentNullException(nameof(taskDescription));
            
            if (taskDescription.Length > MaxTaskDescriptionLength)
            {
                throw new ArgumentException($"task description can be at most {MaxTaskDescriptionLength} characters long, but was {taskDescription.Length}", nameof(taskDescription));
            }

            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return null;
            }

            var user = await userRepository.GetById(taskList.OwnerId);

            if (user == null)
            {
                return null;
            }

            if (!user.IsPremium && taskList.Entries.Count >= NonPremiumUserTaskEntryCountLimit)
            {
                throw new ArgumentException($"non-premium user {taskList.OwnerId} can only have at most {NonPremiumUserTaskEntryCountLimit} tasks in their list");
            }

            taskList.AddEntry(taskDescription);
            await taskListRepository.Upsert(taskList);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", taskDescription, taskList.Id);

            await statisticsService.OnTaskAddedToList(taskList, taskList.Entries.Count - 1);

            return taskList;
        }

        public async Task<TaskList?> MarkTaskAsDone(long taskListId, int entryIdx)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return null;
            }

            if (entryIdx < 0 || entryIdx >= taskList.Entries.Count)
            {
                throw new ArgumentException($"entry with index {entryIdx} does not exist");
            }

            taskList.MarkEntryAsDone(entryIdx);

            await taskListRepository.Upsert(taskList);

            logger.LogDebug("marked task list entry '{EntryIdx}' in task list '{TaskListName}' as done", entryIdx, taskList.Name);

            await statisticsService.OnTaskMarkedAsDone(taskList, entryIdx);

            return taskList;
        }

        public Task<IReadOnlyCollection<TaskList>> GetAll() => taskListRepository.GetAll();

        public Task<TaskList?> GetById(long taskListId) => taskListRepository.GetById(taskListId);

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries() => taskListRepository.GetAllWithPendingEntries();

        public async Task<bool> DeleteById(long taskListId)
        {
            var wasDeleted = await taskListRepository.DeleteById(taskListId);

            if (!wasDeleted)
            {
                return false;
            }

            logger.LogDebug("deleted task list '{TaskListId}'", taskListId);

            await statisticsService.OnTaskListDeleted(taskListId);

            return true;
        }
    }
}
