using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateCommandAttribute : Attribute
    {
    }
    
    internal sealed class CommandHandlerValidationDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : notnull
    {
        private readonly ICommandHandler<TCommand, TResponse> handler;

        public CommandHandlerValidationDecorator(ICommandHandler<TCommand, TResponse> handler)
        {
            this.handler = handler;
        }

        public async Task<TResponse> ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);
            return await handler.ExecuteCommand(command, cancellationToken);
        }
    }

    internal sealed class CommandHandlerValidationDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : notnull
    {
        private readonly ICommandHandler<TCommand> handler;

        public CommandHandlerValidationDecorator(ICommandHandler<TCommand> handler)
        {
            this.handler = handler;
        }

        public async Task ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);
            await handler.ExecuteCommand(command, cancellationToken);
        }
    }
}
