using System;
using System.Text.Json;
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

        public bool LogQueryPayload { get; init; } = true;

        public bool LogResponsePayload { get; init; } = true;
    }

    public sealed class QueryHandlerLoggingBehavior<TQuery, TResponse> : IQueryHandlerBehavior<TQuery, TResponse, QueryLoggingBehaviorAttribute>
        where TQuery : notnull
    {
        private readonly ILogger logger;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public QueryHandlerLoggingBehavior(ILoggerFactory loggerFactory, JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
            logger = loggerFactory.CreateLogger($"QueryHandler[{typeof(TQuery).Name},{typeof(TResponse).Name}]");
        }

        public async Task<TResponse> InterceptQuery(TQuery query,
                                                    QueryHandlerBehaviorNext<TQuery, TResponse> next,
                                                    QueryLoggingBehaviorAttribute attribute,
                                                    CancellationToken cancellationToken)
        {
            try
            {
                if (attribute.LogQueryPayload)
                {
                    logger.LogInformation("Handling query of type {QueryType} with payload {QueryPayload}", typeof(TQuery).Name, Serialize(query));
                }
                else
                {
                    logger.LogInformation("Handling query of type {QueryType}", typeof(TQuery).Name);
                }

                var response = await next(query, cancellationToken);
                
                if (attribute.LogResponsePayload)
                {
                    logger.LogInformation("Handled query of type {QueryType} and got response {ResponsePayload}", typeof(TQuery).Name, Serialize(response));
                }
                else
                {
                    logger.LogInformation("Handled query of type {QueryType}", typeof(TQuery).Name);
                }

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

        private string Serialize<T>(T value) => JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
