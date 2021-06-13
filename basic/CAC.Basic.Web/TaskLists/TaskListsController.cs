using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Domain.TaskListAggregate;
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
        public async Task<CreateNewTaskListResponseDto> CreateNewTaskList(CreateNewTaskListRequestDto request)
        {
            var taskList = await taskListService.CreateNewTaskList(request.OwnerId, request.Name);
            return new(taskList.Id);
        }

        [HttpPost("{taskListId}/tasks")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<AddTaskToListResponseDto> AddTaskToList(TaskListId taskListId, AddTaskToListRequestDto request)
        {
            var (_, entryId) = await taskListService.AddTaskToList(taskListId, request.TaskDescription);
            return new(entryId);
        }

        [HttpPut("{taskListId}/tasks/{entryId}/isDone")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> MarkTaskAsDone(TaskListId taskListId, TaskListEntryId entryId)
        {
            _ = await taskListService.MarkTaskAsDone(taskListId, entryId);
            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAll()
        {
            var lists = await taskListService.GetAll();
            return lists.Select(TaskListDto.FromTaskList).ToList();
        }

        [HttpGet("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<TaskListDto> GetById(TaskListId taskListId)
        {
            var taskList = await taskListService.GetById(taskListId);
            return TaskListDto.FromTaskList(taskList);
        }

        [HttpGet("withPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IReadOnlyCollection<TaskListDto>> GetAllWithPendingEntries()
        {
            var lists = await taskListService.GetAllWithPendingEntries();
            return lists.Select(TaskListDto.FromTaskList).ToList();
        }

        [HttpDelete("{taskListId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteById(TaskListId taskListId)
        {
            _ = await taskListService.DeleteById(taskListId);
            return NoContent();
        }
    }
}
