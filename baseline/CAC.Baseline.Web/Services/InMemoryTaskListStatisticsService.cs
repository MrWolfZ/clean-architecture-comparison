using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Services
{
    internal sealed class InMemoryTaskListStatisticsService : ITaskListStatisticsService
    {
        private readonly TaskListStatistics statistics = new TaskListStatistics();

        public Task OnTaskListCreated(TaskList taskList)
        {
            statistics.NumberOfTaskListsCreated += 1;
            return Task.CompletedTask;
        }

        public Task OnTaskAddedToList(TaskListEntry taskListEntry) => OnTaskListEdited();

        public Task OnTaskMarkedAsDone(long taskListEntryId) => OnTaskListEdited();

        public Task OnTaskListDeleted(long taskListId)
        {
            statistics.NumberOfTaskListsDeleted += 1;
            return Task.CompletedTask;
        }

        public Task<TaskListStatistics> GetStatistics() => Task.FromResult(statistics);

        private Task OnTaskListEdited()
        {
            statistics.NumberOfTimesTaskListsWereEdited += 1;
            return Task.CompletedTask;
        }
    }
}
