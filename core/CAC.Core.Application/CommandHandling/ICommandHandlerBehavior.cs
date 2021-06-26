using System;
using System.Threading;
using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application.CommandHandling
{
    public delegate Task<TResponse> CommandHandlerBehaviorNext<in TCommand, TResponse>(TCommand command, CancellationToken cancellationToken);

    public interface ICommandHandlerBehavior<TCommand, TResponse, in TMarkerAttribute>
        where TCommand : notnull
        where TMarkerAttribute : Attribute
    {
        Task<TResponse> InterceptCommand(TCommand command, CommandHandlerBehaviorNext<TCommand, TResponse> next, TMarkerAttribute attribute, CancellationToken cancellationToken);
    }
}
