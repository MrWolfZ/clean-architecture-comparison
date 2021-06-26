using System.Threading.Tasks;
using CAC.CQS.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.TaskLists.Queries.GetAllTaskListsWithPendingEntries
{
    [TestFixture]
    public sealed class GetAllTaskListsWithPendingEntriesQueryWebApiTests : GetAllTaskListsWithPendingEntriesQueryTests
    {
        protected override async Task<GetAllTaskListsWithPendingEntriesQueryResponse> ExecuteQuery(GetAllTaskListsWithPendingEntriesQuery query)
        {
            return await ExecuteQueryWithHttp("taskLists/getAllTaskListsWithPendingEntries");
        }

        protected override async Task AssertQueryFailure(GetAllTaskListsWithPendingEntriesQuery query, ExpectedQueryFailure expectedFailure)
        {
            await AssertQueryFailureWithHttp("taskLists/getAllTaskListsWithPendingEntries", expectedFailure);
        }
    }
}
