using CAC.Core.Domain;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class EntityIdSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (EntityId.IsEntityIdType(context.Type))
            {
                schema.Type = "string";
                schema.Properties.Clear();
                schema.Example = new OpenApiString(EntityId.CreateExampleValue(context.Type));
                schema.AdditionalPropertiesAllowed = true;
            }
        }
    }
}
