using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CAC.Core.Application.CommandHandling
{
    public delegate Task<TResponse> CommandHandlerBehaviorInvocation<TCommand, TResponse>(TCommand command, CommandHandlerBehaviorNext<TCommand, TResponse> next, CancellationToken cancellationToken);

    internal sealed class CommandHandlerBehaviorInvocationDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>, ICommandHandler<TCommand>
        where TCommand : notnull
    {
        private readonly IReadOnlyList<CommandHandlerBehaviorInvocation<TCommand, TResponse>> behaviors;
        private readonly ICommandHandler<TCommand, TResponse> commandHandler;

        public CommandHandlerBehaviorInvocationDecorator(ICommandHandler<TCommand, TResponse> commandHandler, IReadOnlyList<CommandHandlerBehaviorInvocation<TCommand, TResponse>> behaviors)
        {
            this.behaviors = behaviors;
            this.commandHandler = commandHandler;
        }

        Task<TResponse> ICommandHandler<TCommand, TResponse>.ExecuteCommand(TCommand command, CancellationToken cancellationToken) => ExecuteCommand(command, cancellationToken);

        Task ICommandHandler<TCommand>.ExecuteCommand(TCommand command, CancellationToken cancellationToken) => ExecuteCommand(command, cancellationToken);

        private Task<TResponse> ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            var index = 0;

            return ExecuteNextBehavior(command, cancellationToken);

            Task<TResponse> ExecuteNextBehavior(TCommand cmd, CancellationToken token)
            {
                if (index >= behaviors.Count)
                {
                    return commandHandler.ExecuteCommand(cmd, token);
                }
                
                var behavior = behaviors[index++];
                return behavior(cmd, ExecuteNextBehavior, token);
            }
        }
    }
}
