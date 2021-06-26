using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CAC.Core.Application.QueryHandling
{
    public delegate Task<TResponse> QueryHandlerBehaviorInvocation<TQuery, TResponse>(TQuery query, QueryHandlerBehaviorNext<TQuery, TResponse> next, CancellationToken cancellationToken);
    
    internal sealed class QueryHandlerBehaviorInvocationDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : notnull
    {
        private readonly IReadOnlyList<QueryHandlerBehaviorInvocation<TQuery, TResponse>> behaviors;
        private readonly IQueryHandler<TQuery, TResponse> queryHandler;

        public QueryHandlerBehaviorInvocationDecorator(IQueryHandler<TQuery, TResponse> queryHandler, IReadOnlyList<QueryHandlerBehaviorInvocation<TQuery, TResponse>> behaviors)
        {
            this.behaviors = behaviors;
            this.queryHandler = queryHandler;
        }

        public Task<TResponse> ExecuteQuery(TQuery query, CancellationToken cancellationToken)
        {
            var index = 0;

            return ExecuteNextBehavior(query, cancellationToken);

            Task<TResponse> ExecuteNextBehavior(TQuery cmd, CancellationToken token)
            {
                if (index >= behaviors.Count)
                {
                    return queryHandler.ExecuteQuery(cmd, token);
                }

                var behavior = behaviors[index++];
                return behavior(cmd, ExecuteNextBehavior, token);
            }
        }
    }
}
