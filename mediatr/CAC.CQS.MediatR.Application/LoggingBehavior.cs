using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CAC.CQS.MediatR.Application
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, JsonSerializerOptions jsonSerializerOptions)
        {
            this.logger = logger;
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                logger.LogInformation("Handling request of type {RequestType} with payload {RequestPayload}", typeof(TRequest).Name, Serialize(request));

                var response = await next();

                logger.LogInformation("Handled request of type {RequestType} and got response {ResponsePayload}", typeof(TRequest).Name, Serialize(response));

                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred while handling request of type {RequestType}!", typeof(TRequest).Name);
                throw;
            }
        }

        private string Serialize<T>(T value) => JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
