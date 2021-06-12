using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Dtos;
using CAC.DDD.Web.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CAC.DDD.Web.Controllers
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListsController : ControllerBase
    {
        private readonly ILogger<TaskListsController> logger;
        private readonly ITaskListRepository taskListRepository;
        private readonly IUserRepository userRepository;

        public TaskListsController(ITaskListRepository taskListRepository,
                                   IUserRepository userRepository,
                                   ILogger<TaskListsController> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
            this.userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<CreateNewTaskListResponseDto>> CreateNewTaskList(CreateNewTaskListRequestDto request)
        {
            var owner = await userRepository.GetById(request.OwnerId);

            if (owner == null)
            {
                return NotFound($"user {request.OwnerId} does not exist");
            }

            var numberOfListsOwnedByOwner = await taskListRepository.GetNumberOfTaskListsByOwner(request.OwnerId);

            var id = await taskListRepository.GenerateId();
            var taskList = TaskList.ForOwner(owner, id, request.Name, numberOfListsOwnedByOwner);

            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("created new task list with name '{Name}' and id '{TaskListId}' for owner '{OwnerId}'...", request.Name, id, taskList.OwnerId);

            return Ok(new CreateNewTaskListResponseDto(id));
        }

        [HttpPost("{taskListId}/tasks")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<AddTaskToListResponseDto>> AddTaskToList(TaskListId taskListId, AddTaskToListRequestDto request)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            var id = await taskListRepository.GenerateEntryId();
            var newEntry = TaskListEntry.ForAddingToTaskList(taskList.Id, id, request.TaskDescription);
            taskList = taskList.AddEntry(newEntry);
            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListId}'", request.TaskDescription, taskList.Id);

            return Ok(new AddTaskToListResponseDto(id));
        }

        [HttpPut("{taskListId}/tasks/{entryId}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(TaskListId taskListId, TaskListEntryId entryId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list '{taskListId}' does not exist");
            }

            if (taskList.Entries.All(e => e.Id != entryId))
            {
                return NotFound($"entry '{entryId}' does not exist");
            }

            taskList = taskList.MarkEntryAsDone(entryId);

            taskList = await taskListRepository.Upsert(taskList);

            logger.LogDebug("marked task list entry '{EntryId}' in task list '{TaskListName}' as done", entryId, taskList.Name);

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListRepository.GetAll();
            return lists.Select(TaskListDto.FromTaskListEntry).ToList();
        }

        [HttpGet("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<TaskListDto>> GetById(TaskListId taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);
            return taskList == null ? NotFound($"task list {taskListId} does not exist") : Ok(TaskListDto.FromTaskListEntry(taskList));
        }

        [HttpGet("withPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingEntries()
        {
            var lists = await taskListRepository.GetAllWithPendingEntries();
            return lists.Select(TaskListDto.FromTaskListEntry).ToList();
        }

        [HttpDelete("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(TaskListId taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);

            if (taskList == null)
            {
                return NotFound($"task list '{taskListId}' does not exist");
            }

            taskList = taskList.MarkAsDeleted();

            _ = await taskListRepository.Upsert(taskList);

            logger.LogDebug("deleted task list '{TaskListId}'", taskListId);

            return NoContent();
        }
    }
}
