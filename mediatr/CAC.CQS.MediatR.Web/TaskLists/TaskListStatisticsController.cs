using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.MediatR.Web.TaskLists
{
    [ApiController]
    [Route("taskListStatistics")]
    public class TaskListStatisticsController : ControllerBase
    {
        private readonly ITaskListStatisticsRepository repository;

        public TaskListStatisticsController(ITaskListStatisticsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<TaskListStatistics> GetAll()
        {
            return await repository.Get();
        }
    }
}