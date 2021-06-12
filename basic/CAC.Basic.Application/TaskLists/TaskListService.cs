using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAC.Basic.Application.Users;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Basic.Domain.UserAggregate;
using CAC.Core.Application.Exceptions;
using CAC.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CAC.Basic.Application.TaskLists
{
    internal sealed class TaskListService : ITaskListService
    {
        public const int MaxTaskListNameLength = 64;
        public const int MaxTaskDescriptionLength = 256;

        private readonly ILogger<TaskListService> logger;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListService(ITaskListRepository taskListRepository,
                               IUserRepository userRepository,
                               ILogger<TaskListService> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
            this.userRepository = userRepository;
        }

        public async Task<TaskList> CreateNewTaskList(UserId ownerId, string name)
        {
            if (name.Length > MaxTaskListNameLength)
            {
                throw new ValidationException($"task list name must not be longer than {MaxTaskListNameLength} characters, but it was {name.Length} characters long");
            }
            
            var owner = await userRepository.GetById(ownerId);

            if (owner == null)
            {
                throw new DomainEntityNotFoundException(ownerId, $"user {ownerId} does not exist");
            }

            var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(ownerId);

            var id = await taskListRepository.GenerateId();
            var taskList = TaskList.New(id, owner, name, numberOfListsOwnedByOwner);

            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{TaskListId}' for owner '{OwnerId}'...", name, id, taskList.OwnerId);

            return taskList;
        }

        public async Task<(TaskList taskList, TaskListEntryId entryId)> AddTaskToList(TaskListId taskListId, string taskDescription)
        {
            if (taskDescription.Length > MaxTaskDescriptionLength)
            {
                throw new ValidationException($"task list entry description must not be longer than {MaxTaskDescriptionLength} characters, but it was {taskDescription.Length} characters long");
            }
            
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(taskListId, $"task list {taskListId} does not exist");
            }

            var user = await userRepository.GetById(taskList.OwnerId);

            if (user == null)
            {
                throw new DomainEntityNotFoundException(taskList.OwnerId, $"user {taskList.OwnerId} does not exist");
            }

            var id = await taskListRepository.GenerateEntryId();
            taskList = taskList.AddEntry(id, taskDescription, user);
            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", taskDescription, taskList.Id);

            return (taskList, id);
        }

        public async Task<TaskList> MarkTaskAsDone(TaskListId taskListId, TaskListEntryId entryId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(taskListId, $"task list '{taskListId}' does not exist");
            }

            if (taskList.Entries.All(e => e.Id != entryId))
            {
                throw new DomainEntityNotFoundException(entryId, $"entry '{entryId}' does not exist");
            }

            taskList = taskList.MarkEntryAsDone(entryId);

            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("marked task list entry '{EntryId}' in task list '{TaskListName}' as done", entryId, taskList.Name);

            return taskList;
        }

        public Task<IReadOnlyCollection<TaskList>> GetAll() => taskListRepository.GetAll();

        public async Task<TaskList> GetById(TaskListId taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(taskListId, $"task list {taskListId} does not exist");
            }

            return taskList;
        }

        public Task<IReadOnlyCollection<TaskList>> GetAllWithPendingEntries() => taskListRepository.GetAllWithPendingEntries();

        public async Task<TaskList> DeleteById(TaskListId taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                throw new DomainEntityNotFoundException(taskListId, $"task list '{taskListId}' does not exist");
            }

            taskList = taskList.MarkAsDeleted();

            _ = await taskListRepository.Upsert(taskList);

            logger.LogDebug("deleted task list '{TaskListId}'", taskListId);

            return taskList;
        }
    }
}
