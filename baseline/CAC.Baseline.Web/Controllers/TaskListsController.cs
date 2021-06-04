using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.Baseline.Web.Data;
using CAC.Baseline.Web.Model;
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
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListsController(ITaskListRepository taskListRepository,
                                   IUserRepository userRepository,
                                   ITaskListStatisticsService statisticsService,
                                   ILogger<TaskListsController> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
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
                await taskListRepository.Upsert(taskList);

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
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            var user = await userRepository.GetById(taskList.OwnerId);

            if (user == null)
            {
                return Conflict($"user {taskList.OwnerId} does not exist");
            }

            if (!user.IsPremium && taskList.Entries.Count >= NonPremiumUserTaskEntryCountLimit)
            {
                return Conflict($"non-premium user {taskList.OwnerId} can only have at most {NonPremiumUserTaskEntryCountLimit} tasks in their list");
            }

            taskList.AddEntry(request.TaskDescription);
            await taskListRepository.Upsert(taskList);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", request.TaskDescription, taskList.Id);

            await statisticsService.OnTaskAddedToList(taskList, taskList.Entries.Count - 1);

            return NoContent();
        }

        [HttpPut("{taskListId:long}/tasks/{entryIdx:int}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(long taskListId, int entryIdx)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            if (entryIdx < 0 || entryIdx >= taskList.Entries.Count)
            {
                return BadRequest($"entry with index {entryIdx} does not exist");
            }

            taskList.MarkEntryAsDone(entryIdx);

            await taskListRepository.Upsert(taskList);

            logger.LogDebug("marked task list entry '{EntryIdx}' in task list '{TaskListName}' as done", entryIdx, taskList.Name);

            await statisticsService.OnTaskMarkedAsDone(taskList, entryIdx);

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListRepository.GetAll();
            return lists.Select(l => new TaskListDto(l.Id, l.Name, l.Entries)).ToList();
        }

        [HttpGet("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<TaskListDto>> GetById(long taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);
            return taskList == null ? NotFound($"task list {taskListId} does not exist") : Ok(new TaskListDto(taskList.Id, taskList.Name, taskList.Entries));
        }

        [HttpGet("withPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingEntries()
        {
            var lists = await taskListRepository.GetAllWithPendingEntries();
            return lists.Select(l => new TaskListDto(l.Id, l.Name, l.Entries)).ToList();
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
    }
}
