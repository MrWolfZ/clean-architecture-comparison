using System;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Persistence;

namespace CAC.Baseline.Web.Services
{
    internal sealed class TaskListStatisticsService : ITaskListStatisticsService
    {
        private readonly ITaskListStatisticsRepository repository;

        public TaskListStatisticsService(ITaskListStatisticsRepository repository)
        {
            this.repository = repository;
        }

        public Task OnTaskListCreated(TaskList taskList) => UpdateStatistics(s => s with { NumberOfTaskListsCreated = s.NumberOfTaskListsCreated + 1 });

        public Task OnTaskAddedToList(TaskListEntry taskListEntry) => OnTaskListEdited();

        public Task OnTaskMarkedAsDone(long taskListEntryId) => OnTaskListEdited();

        public Task OnTaskListDeleted(long taskListId) => UpdateStatistics(s => s with { NumberOfTaskListsDeleted = s.NumberOfTaskListsDeleted + 1 });

        public Task<TaskListStatistics> GetStatistics() => repository.Get();

        private Task OnTaskListEdited() => UpdateStatistics(s => s with { NumberOfTimesTaskListsWereEdited = s.NumberOfTimesTaskListsWereEdited + 1 });

        private async Task UpdateStatistics(Func<TaskListStatistics, TaskListStatistics> updateFn)
        {
            var stored = await repository.Get();
            var updated = updateFn(stored);
            await repository.Upsert(updated);
        }
    }
}
