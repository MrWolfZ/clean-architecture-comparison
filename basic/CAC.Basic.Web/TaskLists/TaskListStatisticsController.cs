using System.Threading.Tasks;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Domain.TaskLists;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Basic.Web.TaskLists
{
    [ApiController]
    [Route("taskListStatistics")]
    public class TaskListStatisticsController : ControllerBase
    {
        private readonly ITaskListStatisticsService statisticsService;

        public TaskListStatisticsController(ITaskListStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<TaskListStatistics> GetAll()
        {
            return await statisticsService.GetStatistics();
        }
    }
}
