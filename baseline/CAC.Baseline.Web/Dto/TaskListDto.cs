using System.Collections.Generic;

namespace CAC.Baseline.Web.Dto
{
    public sealed record TaskListDto(long Id, string Name, IList<TaskListEntryDto> Entries);

    public sealed record TaskListEntryDto(long Id, string Description, bool IsDone);
}
