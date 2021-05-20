using CAC.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Core.Web
{
    internal sealed class DomainValidationProblemDetails : ProblemDetails
    {
        private DomainValidationProblemDetails(DomainValidationException ex)
        {
            Title = ex.Message;
            Detail = ex.Details;
            Status = StatusCodes.Status409Conflict;
            Type = ex.Type;
            Instance = ex.EntityId.Value;
        }

        public static DomainValidationProblemDetails New(DomainValidationException ex) => new DomainValidationProblemDetails(ex);
    }
}
