using System.Threading.Tasks;

namespace CAC.CQS.Domain
{
    public interface ICommandHandler<in TCommand, TResponse>
    {
        Task<TResponse> ExecuteCommand(TCommand command);
    }

    public interface ICommandHandler<in TCommand>
    {
        Task ExecuteCommand(TCommand command);
    }
}
