using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application
{
    public sealed record QueryHandlerLoggingOptions(bool LogException);

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogQueryAttribute : Attribute
    {
        public bool LogException { get; init; } = true;
    }

    internal sealed class QueryHandlerLoggingDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : notnull
    {
        private readonly IQueryHandler<TQuery, TResponse> handler;
        private readonly ILogger<QueryHandlerLoggingDecorator<TQuery, TResponse>> logger;
        private readonly QueryHandlerLoggingOptions options;

        public QueryHandlerLoggingDecorator(IQueryHandler<TQuery, TResponse> handler,
                                            ILogger<QueryHandlerLoggingDecorator<TQuery, TResponse>> logger, 
                                            QueryHandlerLoggingOptions options)
        {
            this.handler = handler;
            this.logger = logger;
            this.options = options;
        }

        public async Task<TResponse> ExecuteQuery(TQuery query, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Handling query of type {QueryType}", typeof(TQuery).Name);
                
                var response = await handler.ExecuteQuery(query, cancellationToken);

                logger.LogInformation("Handled query of type {QueryType} and got response of type {ResponseType}", typeof(TQuery).Name, typeof(TResponse).Name);

                return response;
            }
            catch (Exception e)
            {
                if (options.LogException)
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
