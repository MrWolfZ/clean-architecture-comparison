using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

// it makes sense for these classes to be in the same file
#pragma warning disable SA1402

// it makes sense for these classes to be in this order
#pragma warning disable SA1649

namespace CAC.Core.Application
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateQueryAttribute : Attribute
    {
    }

    internal sealed class QueryHandlerValidationDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : notnull
    {
        private readonly IQueryHandler<TQuery, TResponse> handler;

        public QueryHandlerValidationDecorator(IQueryHandler<TQuery, TResponse> handler)
        {
            this.handler = handler;
        }

        public async Task<TResponse> ExecuteQuery(TQuery query, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(query, new(query), true);
            return await handler.ExecuteQuery(query, cancellationToken);
        }
    }
}
