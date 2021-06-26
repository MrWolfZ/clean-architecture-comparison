using System;
using System.Threading;
using System.Threading.Tasks;

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application.QueryHandling
{
    public delegate Task<TResponse> QueryHandlerBehaviorNext<in TQuery, TResponse>(TQuery query, CancellationToken cancellationToken);

    public interface IQueryHandlerBehavior<TQuery, TResponse, in TMarkerAttribute>
        where TQuery : notnull
        where TMarkerAttribute : Attribute
    {
        Task<TResponse> InterceptQuery(TQuery query, QueryHandlerBehaviorNext<TQuery, TResponse> next, TMarkerAttribute attribute, CancellationToken cancellationToken);
    }
}
