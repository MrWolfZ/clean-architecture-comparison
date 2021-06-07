using CAC.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Core.Web
{
    internal sealed class DomainExceptionProblemDetails : ProblemDetails
    {
        public DomainExceptionProblemDetails(DomainEntityException ex, int statusCode)
        {
            Title = ex.Message;
            Detail = ex.Details;
            Status = statusCode;
            Type = ex.Type;
            Instance = ex.EntityId.Value;
        }
    }
}
