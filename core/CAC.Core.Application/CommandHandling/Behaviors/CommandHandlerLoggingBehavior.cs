using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application.CommandHandling.Behaviors
{
    public sealed class CommandLoggingBehaviorAttribute : CommandHandlerBehaviorAttribute
    {
        public bool LogException { get; init; } = true;
    }

    public sealed class CommandHandlerLoggingBehavior<TCommand, TResponse> : ICommandHandlerBehavior<TCommand, TResponse, CommandLoggingBehaviorAttribute>
        where TCommand : notnull
    {
        private readonly ILogger logger;

        public CommandHandlerLoggingBehavior(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger($"CommandHandler[{typeof(TCommand).Name},{typeof(TResponse).Name}]");
        }

        public async Task<TResponse> InterceptCommand(TCommand command, 
                                                      CommandHandlerBehaviorNext<TCommand, TResponse> next, 
                                                      CommandLoggingBehaviorAttribute attribute, 
                                                      CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Handling command of type {CommandType}", typeof(TCommand).Name);

                var response = await next(command, cancellationToken);

                logger.LogInformation("Handled command of type {CommandType} and got response of type {ResponseType}", typeof(TCommand).Name, typeof(TResponse).Name);

                return response;
            }
            catch (Exception e)
            {
                if (attribute.LogException)
                {
                    logger.LogError(e, "An exception occurred while handling command of type {CommandType}!", typeof(TCommand).Name);
                }
                else
                {
                    logger.LogError("An exception occurred while handling command of type {CommandType}!", typeof(TCommand).Name);
                }
                
                throw;
            }
        }
    }
}
