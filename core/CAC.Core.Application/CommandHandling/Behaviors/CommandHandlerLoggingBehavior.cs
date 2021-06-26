using System;
using System.Text.Json;
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
        
        public bool LogCommandPayload { get; init; } = true;
        
        public bool LogResponsePayload { get; init; } = true;
    }

    public sealed class CommandHandlerLoggingBehavior<TCommand, TResponse> : ICommandHandlerBehavior<TCommand, TResponse, CommandLoggingBehaviorAttribute>
        where TCommand : notnull
    {
        private readonly ILogger logger;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public CommandHandlerLoggingBehavior(ILoggerFactory loggerFactory, JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
            logger = loggerFactory.CreateLogger($"CommandHandler[{typeof(TCommand).Name},{typeof(TResponse).Name}]");
        }

        public async Task<TResponse> InterceptCommand(TCommand command, 
                                                      CommandHandlerBehaviorNext<TCommand, TResponse> next, 
                                                      CommandLoggingBehaviorAttribute attribute, 
                                                      CancellationToken cancellationToken)
        {
            try
            {
                if (attribute.LogCommandPayload)
                {
                    logger.LogInformation("Handling command of type {CommandType} with payload {CommandPayload}", typeof(TCommand).Name, Serialize(command));
                }
                else
                {
                    logger.LogInformation("Handling command of type {CommandType}", typeof(TCommand).Name);
                }

                var response = await next(command, cancellationToken);
                
                if (attribute.LogResponsePayload)
                {
                    logger.LogInformation("Handled command of type {CommandType} and got response {ResponsePayload}", typeof(TCommand).Name, Serialize(response));
                }
                else
                {
                    logger.LogInformation("Handled command of type {CommandType}", typeof(TCommand).Name);
                }

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

        private string Serialize<T>(T value) => JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
