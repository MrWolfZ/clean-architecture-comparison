using System.Threading;
using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application.CommandHandling
{
    public interface ICommandHandler
    {
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler
        where TCommand : notnull
    {
        Task ExecuteCommand(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand, TResponse> : ICommandHandler
        where TCommand : notnull
    {
        Task<TResponse> ExecuteCommand(TCommand command, CancellationToken cancellationToken);
    }
}
