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

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            logger.LogInformation("Handling request of type {RequestType}", typeof(TRequest).Name);
            
            var response = await next();
            
            logger.LogInformation("Handling request of type {RequestType} and got response of type {ResponseType}", typeof(TRequest).Name, typeof(TResponse).Name);

            return response;
        }
    }
}
