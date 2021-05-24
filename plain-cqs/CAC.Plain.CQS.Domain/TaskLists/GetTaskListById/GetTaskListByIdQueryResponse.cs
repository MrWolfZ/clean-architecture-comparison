using System.Collections.Immutable;
using System.Linq;

namespace CAC.Plain.CQS.Domain.TaskLists.GetTaskListById
{
    public sealed record GetTaskListByIdQueryResponse(TaskListId Id, string Name, ValueList<TaskListItemDto> Items)
    {
        internal GetTaskListByIdQueryResponse(TaskList taskList)
            : this(taskList.Id, taskList.Name, taskList.Items.Select(i => new TaskListItemDto(i.Description, i.IsDone)).ToValueList())
        {
        }
    }

    public sealed record TaskListItemDto(string Description, bool IsDone);
}
