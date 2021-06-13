using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application
{
    public interface IQueryHandler
    {
    }

    public interface IQueryHandler<in TQuery, TResponse> : IQueryHandler
    {
        Task<TResponse> ExecuteQuery(TQuery taskListId);
    }
}
