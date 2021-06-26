using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists.GetAllTaskLists;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Queries.GetAllTaskLists
{
    [TestFixture]
    public sealed class GetAllTaskListsQueryWebApiTests : GetAllTaskListsQueryTests
    {
        protected override async Task<GetAllTaskListsQueryResponse> ExecuteQuery(GetAllTaskListsQuery query)
        {
            return await ExecuteQueryWithHttp("taskLists/getAllTaskLists");
        }

        protected override async Task AssertQueryFailure(GetAllTaskListsQuery query, ExpectedQueryFailure expectedFailure)
        {
            await AssertQueryFailureWithHttp("taskLists/getAllTaskLists", expectedFailure);
        }
    }
}
