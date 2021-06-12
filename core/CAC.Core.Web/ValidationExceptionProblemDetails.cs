using CAC.Core.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAC.Core.Web
{
    internal sealed class ValidationExceptionProblemDetails : ProblemDetails
    {
        public ValidationExceptionProblemDetails(ValidationException ex)
        {
            Title = ex.Message;
            Detail = ex.Details;
            Status = StatusCodes.Status400BadRequest;
            Type = ex.Type;
            Instance = ex.EntityId?.Value;
        }
    }
}