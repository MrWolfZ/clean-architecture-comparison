using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Basic.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListsController : ControllerBase
    {
        private readonly ITaskListService taskListService;

        public TaskListsController(ITaskListService taskListService)
        {
            this.taskListService = taskListService;
        }

        [HttpPost]
        public async Task<ActionResult<CreateNewTaskListResponseDto>> CreateNewTaskList(CreateNewTaskListRequestDto request)
        {
            try
            {
                var taskList = await taskListService.CreateNewTaskList(request.OwnerId, request.Name);

                if (taskList == null)
                {
                    return NotFound($"user {request.OwnerId} does not exist");
                }

                return Ok(new CreateNewTaskListResponseDto(taskList.Id));
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
            try
            {
                var taskList = await taskListService.AddTaskToList(taskListId, request.TaskDescription);

                if (taskList == null)
                {
                    return NotFound($"task list {taskListId} does not exist");
                }

                return NoContent();
            }
            catch (ArgumentException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPut("{taskListId:long}/tasks/{entryIdx:int}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(long taskListId, int entryIdx)
        {
            try
            {
                var taskList = await taskListService.MarkTaskAsDone(taskListId, entryIdx);

                if (taskList == null)
                {
                    return NotFound($"task list {taskListId} does not exist");
                }

                return NoContent();
            }
            catch (ArgumentException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListService.GetAll();
            return lists.Select(TaskListDto.FromTaskList).ToList();
        }

        [HttpGet("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<TaskListDto>> GetById(long taskListId)
        {
            var taskList = await taskListService.GetById(taskListId);
            return taskList == null ? NotFound($"task list {taskListId} does not exist") : Ok(TaskListDto.FromTaskList(taskList));
        }

        [HttpGet("withPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingEntries()
        {
            var lists = await taskListService.GetAllWithPendingEntries();
            return lists.Select(TaskListDto.FromTaskList).ToList();
        }

        [HttpDelete("{taskListId:long}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(long taskListId)
        {
            var wasDeleted = await taskListService.DeleteById(taskListId);

            if (!wasDeleted)
            {
                return NotFound($"task list {taskListId} does not exist");
            }

            return NoContent();
        }
    }
}
