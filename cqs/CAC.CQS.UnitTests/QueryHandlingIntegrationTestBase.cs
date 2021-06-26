using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Application.QueryHandling;
using CAC.Core.Domain.Exceptions;
using CAC.Core.TestUtilities;
using NUnit.Framework;

namespace CAC.CQS.UnitTests
{
    public abstract class QueryHandlingIntegrationTestBase<TQuery, TResponse> : IntegrationTestBase
        where TQuery : notnull
    {
        protected IQueryHandler<TQuery, TResponse> QueryHandler => Resolve<IQueryHandler<TQuery, TResponse>>();

        protected virtual async Task<TResponse> ExecuteQuery(TQuery query)
        {
            return await QueryHandler.ExecuteQuery(query, CancellationToken.None);
        }

        protected async Task<TResponse> ExecuteQueryWithHttp(string url)
        {
            var httpResponse = await HttpClient.GetAsync(url);

            await httpResponse.AssertStatusCode(HttpStatusCode.OK);

            var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>(JsonSerializerOptions);

            Assert.IsNotNull(response);

            return response!;
        }

        protected virtual Task AssertQueryFailure(TQuery query, ExpectedQueryFailure expectedFailure)
        {
            var thrownException = Assert.CatchAsync<Exception>(() => ExecuteQuery(query));
            Assert.IsInstanceOf(ToExceptionType(expectedFailure), thrownException);
            return Task.CompletedTask;
        }

        protected async Task AssertQueryFailureWithHttp(string url, ExpectedQueryFailure expectedFailure)
        {
            var httpResponse = await HttpClient.GetAsync(url);
            await httpResponse.AssertStatusCode(ToHttpStatusCode(expectedFailure));
        }

        protected HttpStatusCode ToHttpStatusCode(ExpectedQueryFailure expectedQueryFailure) => expectedQueryFailure switch
        {
            ExpectedQueryFailure.EntityNotFound => HttpStatusCode.NotFound,
            _ => throw new ArgumentOutOfRangeException(nameof(expectedQueryFailure), expectedQueryFailure, null),
        };

        protected Type ToExceptionType(ExpectedQueryFailure expectedQueryFailure) => expectedQueryFailure switch
        {
            ExpectedQueryFailure.EntityNotFound => typeof(DomainEntityNotFoundException),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedQueryFailure), expectedQueryFailure, null),
        };

        protected enum ExpectedQueryFailure
        {
            EntityNotFound,
        }
    }
}
