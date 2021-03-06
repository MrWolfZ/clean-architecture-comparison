using System.Threading;
using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application.QueryHandling
{
    public interface IQueryHandler
    {
    }

    public interface IQueryHandler<in TQuery, TResponse> : IQueryHandler
        where TQuery : notnull
    {
        Task<TResponse> ExecuteQuery(TQuery query, CancellationToken cancellationToken);
    }
}
