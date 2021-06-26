using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application.QueryHandling.Behaviors
{
    public sealed class QueryValidationBehaviorAttribute : QueryHandlerBehaviorAttribute
    {
    }

    public sealed class QueryHandlerValidationBehavior<TQuery, TResponse> : IQueryHandlerBehavior<TQuery, TResponse, QueryValidationBehaviorAttribute>
        where TQuery : notnull
    {
        public async Task<TResponse> InterceptQuery(TQuery query,
                                                    QueryHandlerBehaviorNext<TQuery, TResponse> next,
                                                    QueryValidationBehaviorAttribute attribute,
                                                    CancellationToken cancellationToken)
        {
            Validator.ValidateObject(query, new(query), true);
            return await next(query, cancellationToken);
        }
    }
}
