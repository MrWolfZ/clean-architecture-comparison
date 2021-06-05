using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.Baseline.Web.Dto;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Persistence;
using CAC.Baseline.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CAC.Baseline.Web.Controllers
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListsController : ControllerBase
    {
        private const int NonPremiumUserTaskEntryCountLimit = 5;

        private readonly ILogger<TaskListsController> logger;
        private readonly ITaskListStatisticsService statisticsService;
        private readonly ITaskListEntryRepository taskListEntryRepository;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListsController(ITaskListRepository taskListRepository,
                                   ITaskListEntryRepository taskListEntryRepository,
                                   IUserRepository userRepository,
                                   ITaskListStatisticsService statisticsService,
                                   ILogger<TaskListsController> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
            this.taskListEntryRepository = taskListEntryRepository;
            this.statisticsService = statisticsService;
            this.userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<CreateNewTaskListResponseDto>> CreateNewTaskList(CreateNewTaskListRequestDto request)
        {
            var user = await userRepository.GetById(request.OwnerId);

            if (user == null)
            {
                return NotFound($"user {request.OwnerId} does not exist");
            }

            if (!user.IsPremium)
            {
                var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(request.OwnerId);

                if (numberOfListsOwnedByOwner > 0)
                {
                    return Conflict($"non-premium user {request.OwnerId} already owns a task list");
                }
            }

            var id = await taskListRepository.GenerateId();
            var taskList = new TaskList(id, request.OwnerId, request.Name);

            try
            {
                await taskListRepository.Store(taskList);

                logger.LogDebug("created new task list with name '{Name}' and id '{TaskListId}' for owner '{OwnerId}'...", request.Name, id, taskList.OwnerId);

                await statisticsService.OnTaskListCreated(taskList);

                return Ok(new CreateNewTaskListResponseDto(id));
            }
            catch (ArgumentException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPost("{taskListId:long}/tasks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddTaskToList(long taskListId, AddTaskToListRequestDto request)
        {
            var ownerId = await taskListRepository.GetOwnerId(taskListId);

            if (ownerId == null)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            var user = await userRepository.GetById(ownerId.Value);

            if (user == null)
            {
                return Conflict($"user {ownerId} does not exist");
            }

            var nrOfEntries = await taskListEntryRepository.GetNumberOfEntriesForTaskList(taskListId);

            if (!user.IsPremium && nrOfEntries >= NonPremiumUserTaskEntryCountLimit)
            {
                return Conflict($"non-premium user {ownerId} can only have at most {NonPremiumUserTaskEntryCountLimit} tasks in their list");
            }

            var id = await taskListEntryRepository.GenerateId();
            var entry = new TaskListEntry(id, taskListId, request.TaskDescription, false);
            
            await taskListEntryRepository.Store(entry);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", request.TaskDescription, taskListId);

            await statisticsService.OnTaskAddedToList(entry);

            return NoContent();
        }

        [HttpPut("{taskListId:long}/tasks/{entryId:int}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(long taskListId, long entryId)
        {
            if (!await taskListRepository.Exists(taskListId))
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            var wasSuccess = await taskListEntryRepository.MarkEntryAsDone(entryId);

            if (!wasSuccess)
            {
                return BadRequest($"task list entry {entryId} does not exist");
            }

            logger.LogDebug("marked task list entry '{EntryId}' as done", entryId);

            await statisticsService.OnTaskMarkedAsDone(entryId);

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListRepository.GetAll();
            var entriesByTaskListId = await taskListEntryRepository.GetEntriesForTaskLists(lists.Select(l => l.Id).ToList());
            return lists.Select(l => new TaskListDto(l.Id, l.Name, entriesByTaskListId[l.Id].Select(ToDto).ToList())).ToList();
        }

        [HttpGet("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<TaskListDto>> GetById(long taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list {taskListId} does not exist");
            }
            
            var entries = await taskListEntryRepository.GetEntriesForTaskList(taskListId);
            return Ok(new TaskListDto(taskList.Id, taskList.Name, entries.Select(ToDto).ToList()));
        }

        [HttpGet("withPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingEntries()
        {
            var listIds = await taskListEntryRepository.GetIdsOfAllTaskListsWithPendingEntries();
            var lists = await taskListRepository.GetByIds(listIds);
            var entriesByTaskListId = await taskListEntryRepository.GetEntriesForTaskLists(lists.Select(l => l.Id).ToList());
            return lists.Select(l => new TaskListDto(l.Id, l.Name, entriesByTaskListId[l.Id].Select(ToDto).ToList())).ToList();
        }

        [HttpDelete("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(long taskListId)
        {
            var wasDeleted = await taskListRepository.DeleteById(taskListId);

            if (!wasDeleted)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            logger.LogDebug("deleted task list '{TaskListId}'", taskListId);

            await statisticsService.OnTaskListDeleted(taskListId);

            return NoContent();
        }

        private static TaskListEntryDto ToDto(TaskListEntry entry) => new TaskListEntryDto(entry.Id, entry.Description, entry.IsDone);
    }
}
