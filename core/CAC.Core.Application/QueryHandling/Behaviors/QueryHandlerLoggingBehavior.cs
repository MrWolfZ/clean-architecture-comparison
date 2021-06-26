using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application.QueryHandling.Behaviors
{
    public sealed class QueryLoggingBehaviorAttribute : QueryHandlerBehaviorAttribute
    {
        public bool LogException { get; init; } = true;
    }

    public sealed class QueryHandlerLoggingBehavior<TQuery, TResponse> : IQueryHandlerBehavior<TQuery, TResponse, QueryLoggingBehaviorAttribute>
        where TQuery : notnull
    {
        private readonly ILogger logger;

        public QueryHandlerLoggingBehavior(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger($"QueryHandler[{typeof(TQuery).Name},{typeof(TResponse).Name}]");
        }

        public async Task<TResponse> InterceptQuery(TQuery query,
                                                    QueryHandlerBehaviorNext<TQuery, TResponse> next,
                                                    QueryLoggingBehaviorAttribute attribute,
                                                    CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Handling query of type {QueryType}", typeof(TQuery).Name);

                var response = await next(query, cancellationToken);

                logger.LogInformation("Handled query of type {QueryType} and got response of type {ResponseType}", typeof(TQuery).Name, typeof(TResponse).Name);

                return response;
            }
            catch (Exception e)
            {
                if (attribute.LogException)
                {
                    logger.LogError(e, "An exception occurred while handling query of type {QueryType}!", typeof(TQuery).Name);
                }
                else
                {
                    logger.LogError("An exception occurred while handling query of type {QueryType}!", typeof(TQuery).Name);
                }

                throw;
            }
        }
    }
}
