using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Queries.GetTaskListById
{
    [TestFixture]
    public sealed class GetTaskListByIdQueryWebApiTests : GetTaskListByIdQueryTests
    {
        protected override async Task<GetTaskListByIdQueryResponse> ExecuteQuery(GetTaskListByIdQuery query)
        {
            return await ExecuteQueryWithHttp($"taskLists/getTaskListById?TaskListId={query.TaskListId}");
        }

        protected override async Task AssertQueryFailure(GetTaskListByIdQuery query, ExpectedQueryFailure expectedFailure)
        {
            await AssertQueryFailureWithHttp($"taskLists/getTaskListById?TaskListId={query.TaskListId}", expectedFailure);
        }
    }
}
