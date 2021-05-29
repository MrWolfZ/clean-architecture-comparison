using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.DDD.Domain.TaskLists;
using Microsoft.AspNetCore.Mvc;

namespace CAC.DDD.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListController : ControllerBase
    {
        private readonly ITaskListService taskListService;

        public TaskListController(ITaskListService taskListService) => this.taskListService = taskListService;

        [HttpPost]
        public async Task<ActionResult<CreateNewTaskListResponse>> CreateNewTaskList(CreateNewTaskListRequest request)
        {
            var taskListId = await taskListService.CreateNewTaskList(request.Name);
            return Ok(new CreateNewTaskListResponse(taskListId));
        }

        [HttpPost("{taskListId}/tasks")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddTaskToList(TaskListId taskListId, AddTaskToListRequest request)
        {
            var wasFound = await taskListService.AddItemToTaskList(taskListId, request.TaskDescription);
            return wasFound ? NoContent() : NotFound();
        }

        [HttpPut("{taskListId}/tasks/{taskListItemIdx:int}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(TaskListId taskListId, int taskListItemIdx)
        {
            var wasFound = await taskListService.MarkTaskListItemAsDone(taskListId, taskListItemIdx);
            return wasFound ? NoContent() : NotFound();
        }

        [HttpGet("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GetTaskListByIdQueryResponse>> GetTaskListById(TaskListId taskListId)
        {
            var taskList = await taskListService.GetTaskListById(taskListId);
            return taskList == null ? NotFound() : Ok(new GetTaskListByIdQueryResponse(taskList));
        }

        // disable a few warnings and hints that are irrelevant for DTOs
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
#pragma warning disable CA1034
#pragma warning disable CS8618

        public sealed record CreateNewTaskListRequest
        {
            /// <example>my task list</example>
            [Required]
            public string Name { get; init; }
        }

        public sealed record CreateNewTaskListResponse(TaskListId Id);

        public sealed record AddTaskToListRequest
        {
            /// <example>my task</example>
            [Required]
            public string TaskDescription { get; init; }
        }
        
        public sealed record GetTaskListByIdQueryResponse(TaskListId Id, string Name, ValueList<TaskListItemDto> Items)
        {
            internal GetTaskListByIdQueryResponse(TaskList taskList)
                : this(taskList.Id, taskList.Name, taskList.Items.Select(i => new TaskListItemDto(i.Description, i.IsDone)).ToValueList())
            {
            }
        }

        public sealed record TaskListItemDto(string Description, bool IsDone);
    }
}
