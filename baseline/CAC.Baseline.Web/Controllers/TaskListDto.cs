using System.Collections.Generic;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Controllers
{
    public sealed record TaskListDto(long Id, string Name, IReadOnlyCollection<TaskListItem> Items);
}
