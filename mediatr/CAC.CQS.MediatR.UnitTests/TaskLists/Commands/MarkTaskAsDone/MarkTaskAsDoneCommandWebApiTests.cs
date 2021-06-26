using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists.MarkTaskAsDone;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.MarkTaskAsDone
{
    [TestFixture]
    public sealed class MarkTaskAsDoneCommandWebApiTests : MarkTaskAsDoneCommandTests
    {
        protected override async Task ExecuteCommand(MarkTaskAsDoneCommand command)
        {
            await ExecuteCommandWithHttp(command, "taskLists/markTaskAsDone");
        }

        protected override async Task AssertCommandFailure(MarkTaskAsDoneCommand command, ExpectedCommandFailure expectedFailure)
        {
            await AssertCommandFailureWithHttp(command, "taskLists/markTaskAsDone", expectedFailure);
        }
    }
}
