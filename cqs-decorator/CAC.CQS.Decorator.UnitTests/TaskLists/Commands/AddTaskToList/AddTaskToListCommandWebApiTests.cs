using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists.AddTaskToList;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.AddTaskToList
{
    [TestFixture]
    public sealed class AddTaskToListCommandWebApiTests : AddTaskToListCommandTests
    {
        protected override async Task<AddTaskToListCommandResponse> ExecuteCommand(AddTaskToListCommand command)
        {
            return await ExecuteCommandWithHttp(command, "taskLists/addTaskToList");
        }

        protected override async Task AssertCommandFailure(AddTaskToListCommand command, ExpectedCommandFailure expectedFailure)
        {
            await AssertCommandFailureWithHttp(command, "taskLists/addTaskToList", expectedFailure);
        }
    }
}
