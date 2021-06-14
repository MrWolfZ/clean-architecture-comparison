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
    public sealed record CommandHandlerLoggingOptions(bool LogException);

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogCommandAttribute : Attribute
    {
        public bool LogException { get; init; } = true;
    }
    
    internal sealed class CommandHandlerLoggingDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : notnull
    {
        private readonly ICommandHandler<TCommand, TResponse> handler;
        private readonly ILogger<CommandHandlerLoggingDecorator<TCommand, TResponse>> logger;
        private readonly CommandHandlerLoggingOptions options;

        public CommandHandlerLoggingDecorator(ICommandHandler<TCommand, TResponse> handler,
                                              ILogger<CommandHandlerLoggingDecorator<TCommand, TResponse>> logger,
                                              CommandHandlerLoggingOptions options)
        {
            this.handler = handler;
            this.logger = logger;
            this.options = options;
        }

        public async Task<TResponse> ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Handling command of type {CommandType}", typeof(TCommand).Name);

                var response = await handler.ExecuteCommand(command, cancellationToken);

                logger.LogInformation("Handled command of type {CommandType} and got response of type {ResponseType}", typeof(TCommand).Name, typeof(TResponse).Name);

                return response;
            }
            catch (Exception e)
            {
                if (options.LogException)
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

    internal sealed class CommandHandlerLoggingDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : notnull
    {
        private readonly ICommandHandler<TCommand> handler;
        private readonly ILogger<CommandHandlerLoggingDecorator<TCommand>> logger;
        private readonly CommandHandlerLoggingOptions options;

        public CommandHandlerLoggingDecorator(ICommandHandler<TCommand> handler,
                                              ILogger<CommandHandlerLoggingDecorator<TCommand>> logger,
                                              CommandHandlerLoggingOptions options)
        {
            this.handler = handler;
            this.logger = logger;
            this.options = options;
        }

        public async Task ExecuteCommand(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Handling command of type {CommandType}", typeof(TCommand).Name);

                await handler.ExecuteCommand(command, cancellationToken);

                logger.LogInformation("Handled command of type {CommandType} without response", typeof(TCommand).Name);
            }
            catch (Exception e)
            {
                if (options.LogException)
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
