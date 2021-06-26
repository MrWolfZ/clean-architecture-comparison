using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.CreateNewTaskList
{
    [TestFixture]
    public sealed class CreateNewTaskListCommandWebApiTests : CreateNewTaskListCommandTests
    {
        protected override async Task<CreateNewTaskListCommandResponse> ExecuteCommand(CreateNewTaskListCommand command)
        {
            return await ExecuteCommandWithHttp(command, "taskLists/createNewTaskList");
        }

        protected override async Task AssertCommandFailure(CreateNewTaskListCommand command, ExpectedCommandFailure expectedFailure)
        {
            await AssertCommandFailureWithHttp(command, "taskLists/createNewTaskList", expectedFailure);
        }
    }
}
