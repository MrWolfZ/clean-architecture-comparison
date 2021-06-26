using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.DeleteTaskList
{
    [TestFixture]
    public sealed class DeleteTaskListCommandWebApiTests : DeleteTaskListCommandTests
    {
        protected override async Task ExecuteCommand(DeleteTaskListCommand command)
        {
            await ExecuteCommandWithHttp(command, "taskLists/deleteTaskList");
        }

        protected override async Task AssertCommandFailure(DeleteTaskListCommand command, ExpectedCommandFailure expectedFailure)
        {
            await AssertCommandFailureWithHttp(command, "taskLists/deleteTaskList", expectedFailure);
        }
    }
}
