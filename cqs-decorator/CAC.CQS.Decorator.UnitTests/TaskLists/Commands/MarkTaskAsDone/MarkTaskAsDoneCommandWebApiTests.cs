using System.Threading.Tasks;
using CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone;
using NUnit.Framework;

namespace CAC.CQS.Decorator.UnitTests.TaskLists.Commands.MarkTaskAsDone
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
