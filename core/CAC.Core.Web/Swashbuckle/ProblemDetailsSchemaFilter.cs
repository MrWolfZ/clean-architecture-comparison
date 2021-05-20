using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class ProblemDetailsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(ProblemDetails))
            {
                schema.Properties["type"].Nullable = false;
                schema.Properties["title"].Nullable = false;
                schema.Properties["status"].Nullable = false;
                
                schema.Properties.Add("traceId", schema.Properties["type"]);
            }
        }
    }
}
