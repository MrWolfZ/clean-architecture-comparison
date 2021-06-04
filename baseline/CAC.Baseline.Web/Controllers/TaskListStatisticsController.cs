using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Baseline.Web.Controllers
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
