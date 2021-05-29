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
            try
            {
                var id = await taskListRepository.GenerateId();
                var taskList = new TaskList(id, request.Name);
                await taskListRepository.Upsert(taskList);

                logger.LogDebug("created new task list with name '{Name}' and id '{Id}'...", request.Name, id);

                return Ok(new CreateNewTaskListResponseDto(id));
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{taskListId:long}/tasks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddTaskToList(long taskListId, AddTaskToListRequestDto request)
        {
            try
            {
                var taskList = await taskListRepository.GetById(taskListId);

                if (taskList == null)
                {
                    return NotFound();
                }

                taskList.AddItem(request.TaskDescription);
                await taskListRepository.Upsert(taskList);

                logger.LogDebug("added task list item with description '{Description}' to task list '{TaskListName}'", request.TaskDescription, taskList.Name);

                return NoContent();
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{taskListId:long}/tasks/{taskListItemIdx:int}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(long taskListId, int taskListItemIdx)
        {
            try
            {
                var taskList = await taskListRepository.GetById(taskListId);

                if (taskList == null)
                {
                    return NotFound();
                }

                taskList.MarkItemAsDone(taskListItemIdx);

                await taskListRepository.Upsert(taskList);

                logger.LogDebug("marked task list item '{ItemIdx}' in task list '{TaskListName}' as done", taskListItemIdx, taskList.Name);

                return NoContent();
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListRepository.GetAll();
            return lists.Select(l => new TaskListDto(l.Id, l.Name, l.Items)).ToList();
        }

        [HttpGet("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<TaskListDto>> GetById(long taskListId)
        {
            var taskList = await taskListRepository.GetById(taskListId);
            return taskList == null ? NotFound() : Ok(new TaskListDto(taskList.Id, taskList.Name, taskList.Items));
        }

        [HttpGet("withPendingItems")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingItems()
        {
            var lists = await taskListRepository.GetAllWithPendingItems();
            return lists.Select(l => new TaskListDto(l.Id, l.Name, l.Items)).ToList();
        }
    }
}
