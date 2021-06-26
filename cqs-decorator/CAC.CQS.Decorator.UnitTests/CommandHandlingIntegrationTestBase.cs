using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.CommandHandling;
using CAC.Core.Domain.Exceptions;
using CAC.Core.TestUtilities;
using NUnit.Framework;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

namespace CAC.CQS.Decorator.UnitTests
{
    public abstract class CommandHandlingIntegrationTestBase<TCommand> : CommandHandlingIntegrationTestBase<TCommand, UnitCommandResponse>
        where TCommand : notnull
    {
        protected new ICommandHandler<TCommand> CommandHandler => Resolve<ICommandHandler<TCommand>>();

        protected new virtual async Task ExecuteCommand(TCommand command)
        {
            await CommandHandler.ExecuteCommand(command, CancellationToken.None);
        }

        protected new async Task ExecuteCommandWithHttp(TCommand command, string url)
        {
            var httpResponse = await HttpClient.PostAsJsonAsync(url, command, JsonSerializerOptions);
            await httpResponse.AssertStatusCode(HttpStatusCode.NoContent);
        }

        protected new virtual Task AssertCommandFailure(TCommand command, ExpectedCommandFailure expectedFailure)
        {
            var thrownException = Assert.CatchAsync<Exception>(() => ExecuteCommand(command));
            Assert.IsInstanceOf(ToExceptionType(expectedFailure), thrownException);
            return Task.CompletedTask;
        }
    }

    public abstract class CommandHandlingIntegrationTestBase<TCommand, TResponse> : IntegrationTestBase
        where TCommand : notnull
    {
        protected ICommandHandler<TCommand, TResponse> CommandHandler => Resolve<ICommandHandler<TCommand, TResponse>>();

        protected virtual async Task<TResponse> ExecuteCommand(TCommand command)
        {
            return await CommandHandler.ExecuteCommand(command, CancellationToken.None);
        }

        protected async Task<TResponse> ExecuteCommandWithHttp(TCommand command, string url)
        {
            var httpResponse = await HttpClient.PostAsJsonAsync(url, command, JsonSerializerOptions);

            await httpResponse.AssertStatusCode(HttpStatusCode.OK);

            var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>(JsonSerializerOptions);

            Assert.IsNotNull(response);

            return response!;
        }

        protected virtual Task AssertCommandFailure(TCommand command, ExpectedCommandFailure expectedFailure)
        {
            var thrownException = Assert.CatchAsync<Exception>(() => ExecuteCommand(command));
            Assert.IsInstanceOf(ToExceptionType(expectedFailure), thrownException);
            return Task.CompletedTask;
        }

        protected async Task AssertCommandFailureWithHttp(TCommand command, string url, ExpectedCommandFailure expectedFailure)
        {
            var httpResponse = await HttpClient.PostAsJsonAsync(url, command, JsonSerializerOptions);
            await httpResponse.AssertStatusCode(ToHttpStatusCode(expectedFailure));
        }

        protected HttpStatusCode ToHttpStatusCode(ExpectedCommandFailure expectedCommandFailure) => expectedCommandFailure switch
        {
            ExpectedCommandFailure.EntityNotFound => HttpStatusCode.NotFound,
            ExpectedCommandFailure.InvalidCommand => HttpStatusCode.BadRequest,
            ExpectedCommandFailure.DomainInvariantViolation => HttpStatusCode.Conflict,
            _ => throw new ArgumentOutOfRangeException(nameof(expectedCommandFailure), expectedCommandFailure, null),
        };

        protected Type ToExceptionType(ExpectedCommandFailure expectedCommandFailure) => expectedCommandFailure switch
        {
            ExpectedCommandFailure.EntityNotFound => typeof(DomainEntityNotFoundException),
            ExpectedCommandFailure.InvalidCommand => typeof(ValidationException),
            ExpectedCommandFailure.DomainInvariantViolation => typeof(DomainEntityException),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedCommandFailure), expectedCommandFailure, null),
        };

        protected enum ExpectedCommandFailure
        {
            EntityNotFound,
            InvalidCommand,
            DomainInvariantViolation,
        }
    }

    public sealed record UnitCommandResponse;
}
