using System.Linq;
using System.Net;
using System.Net.Mime;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class AssignContentTypeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses.TryGetValue($"{(int)HttpStatusCode.OK}", out var okResponse))
            {
                var mediaTypesToRemove = okResponse.Content.Keys.Where(mediaType => mediaType != MediaTypeNames.Application.Json).ToList();

                foreach (var mediaType in mediaTypesToRemove)
                {
                    okResponse.Content.Remove(mediaType);
                }
            }
        }
    }
}
