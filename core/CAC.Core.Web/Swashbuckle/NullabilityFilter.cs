using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class NullabilityFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var isOurType = context.Type.Namespace?.StartsWith("CAC.") ?? false;

            if (!isOurType)
            {
                return;
            }

            AdjustSchemaNullability(schema);

            void AdjustSchemaNullability(OpenApiSchema s)
            {
                s.Nullable = context.Type.IsNullable(out _);

                foreach (var property in s.Properties.Values)
                {
                    AdjustSchemaNullability(property);
                }
            }
        }
    }
}
