using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application
{
    public interface ICommandHandler
    {
    }
    
    public interface ICommandHandler<in TCommand, TResponse> : ICommandHandler
    {
        Task<TResponse> ExecuteCommand(TCommand command);
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler
    {
        Task ExecuteCommand(TCommand command);
    }
}
