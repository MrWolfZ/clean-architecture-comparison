using System;
using System.Threading.Tasks;
using CAC.DDD.Web.Domain;
using CAC.DDD.Web.Domain.TaskListAggregate;
using CAC.DDD.Web.Persistence;

namespace CAC.DDD.Web.Services
{
    internal sealed class TaskListStatisticsService : ITaskListStatisticsService
    {
        private readonly ITaskListStatisticsRepository repository;

        public TaskListStatisticsService(ITaskListStatisticsRepository repository)
        {
            this.repository = repository;
        }

        public Task OnTaskListCreated(TaskList taskList) => UpdateStatistics(s => s with { NumberOfTaskListsCreated = s.NumberOfTaskListsCreated + 1 });

        public Task OnTaskAddedToList(TaskList taskList, TaskListEntryId taskListEntryId) => OnTaskListEdited();

        public Task OnTaskMarkedAsDone(TaskList taskList, TaskListEntryId taskListEntryId) => OnTaskListEdited();

        public Task OnTaskListDeleted(TaskListId taskListId) => UpdateStatistics(s => s with { NumberOfTaskListsDeleted = s.NumberOfTaskListsDeleted + 1 });

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
