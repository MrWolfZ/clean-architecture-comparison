using System.Threading.Tasks;
using CAC.DDD.Web.Domain;
using CAC.DDD.Web.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace CAC.DDD.Web.Controllers
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
