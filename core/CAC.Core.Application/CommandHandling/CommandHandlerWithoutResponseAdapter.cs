using System.Threading;
using System.Threading.Tasks;

namespace CAC.Core.Application.CommandHandling
{
    internal sealed class CommandHandlerWithoutResponseAdapter<TCommand> : ICommandHandler<TCommand, UnitCommandResponse>
        where TCommand : notnull
    {
        private readonly ICommandHandler<TCommand> commandHandler;

        public CommandHandlerWithoutResponseAdapter(ICommandHandler<TCommand> commandHandler)
        {
            this.commandHandler = commandHandler;
        }

        public async Task<UnitCommandResponse> ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            await commandHandler.ExecuteCommand(command, cancellationToken);
            return UnitCommandResponse.Instance;
        }
    }

    internal sealed record UnitCommandResponse
    {
        public static readonly UnitCommandResponse Instance = new();
    }
}
