using System;
using System.IO;
using System.Reflection;
using CAC.Core.Web.Swashbuckle;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void ConfigureCore(this SwaggerGenOptions options)
        {
            options.OperationFilter<EntityIdOperationFilter>();
            options.OperationFilter<AssignContentTypeOperationFilter>();
            options.OperationFilter<CommonCommonResponsesOperationFilter>();

            options.SchemaFilter<EntityIdSchemaFilter>();
            options.SchemaFilter<NullabilityFilter>();
            options.SchemaFilter<ProblemDetailsSchemaFilter>();

            // Use method name as operationId
            options.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? $"{methodInfo.DeclaringType?.Name}.{methodInfo.Name}" : null);
            options.CustomSchemaIds(GenerateSchemaId);

            var xmlFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "CAC.*.xml");

            foreach (var file in xmlFiles)
            {
                options.IncludeXmlComments(file, true);
            }
        }

        private static string GenerateSchemaId(Type t)
        {
            var name = t.Name;

            while (t.DeclaringType != null)
            {
                name = $"{t.DeclaringType.Name}.{name}";
                t = t.DeclaringType;
            }

            return name;
        }
    }
}
