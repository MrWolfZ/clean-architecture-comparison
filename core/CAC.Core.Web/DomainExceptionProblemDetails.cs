using CAC.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Core.Web
{
    internal sealed class DomainExceptionProblemDetails : ProblemDetails
    {
        public DomainExceptionProblemDetails(DomainEntityException ex)
        {
            Title = ex.Message;
            Detail = ex.Details;
            Status = StatusCodes.Status409Conflict;
            Type = ex.Type;
            Instance = ex.EntityId.Value;
        }
    }
}
