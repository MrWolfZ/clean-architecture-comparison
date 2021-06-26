using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application.CommandHandling.Behaviors
{
    public sealed class CommandValidationBehaviorAttribute : CommandHandlerBehaviorAttribute
    {
    }

    public sealed class CommandHandlerValidationBehavior<TCommand, TResponse> : ICommandHandlerBehavior<TCommand, TResponse, CommandValidationBehaviorAttribute>
        where TCommand : notnull
    {
        public async Task<TResponse> InterceptCommand(TCommand command,
                                                      CommandHandlerBehaviorNext<TCommand, TResponse> next,
                                                      CommandValidationBehaviorAttribute attribute,
                                                      CancellationToken cancellationToken)
        {
            Validator.ValidateObject(command, new(command), true);
            return await next(command, cancellationToken);
        }
    }
}
