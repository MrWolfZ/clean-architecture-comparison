using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.Baseline.Web.Data;
using CAC.Baseline.Web.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CAC.Baseline.Web.Controllers
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListsController : ControllerBase
    {
        private readonly ILogger<TaskListsController> logger;
        private readonly ITaskListRepository taskListRepository;

        public TaskListsController(ITaskListRepository taskListRepository, ILogger<TaskListsController> logger)
        {
            this.taskListRepository = taskListRepository;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CreateNewTaskListResponseDto>> CreateNewTaskList(CreateNewTaskListRequestDto request)
        {
            var id = await taskListRepository.GenerateId();
            var taskList = new TaskList(id, request.Name);

            try
            {
                await taskListRepository.Upsert(taskList);

                logger.LogDebug("created new task list with name '{Name}' and id '{Id}'...", request.Name, id);

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
                return NotFound();
            }

            taskList.AddEntry(request.TaskDescription);
            await taskListRepository.Upsert(taskList);

            logger.LogDebug("added task list entry with description '{Description}' to task list '{TaskListName}'", request.TaskDescription, taskList.Name);

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
                return NotFound();
            }

            if (entryIdx < 0 || entryIdx >= taskList.Entries.Count)
            {
                return BadRequest($"entry with index {entryIdx} does not exist");
            }

            taskList.MarkEntryAsDone(entryIdx);

            await taskListRepository.Upsert(taskList);

            logger.LogDebug("marked task list entry '{EntryIdx}' in task list '{TaskListName}' as done", entryIdx, taskList.Name);

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
            return taskList == null ? NotFound() : Ok(new TaskListDto(taskList.Id, taskList.Name, taskList.Entries));
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
            return wasDeleted ? NoContent() : NotFound();
        }
    }
}
