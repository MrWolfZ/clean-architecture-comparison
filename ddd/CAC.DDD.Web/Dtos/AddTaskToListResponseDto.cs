using CAC.DDD.Web.Domain.TaskListAggregate;

namespace CAC.DDD.Web.Dtos
{
    public sealed record AddTaskToListResponseDto(TaskListEntryId EntryId);
}