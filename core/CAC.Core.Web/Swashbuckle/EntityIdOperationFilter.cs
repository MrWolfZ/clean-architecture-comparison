using System.Linq;
using CAC.Core.Domain;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class EntityIdOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionParameters = context.ApiDescription.ActionDescriptor.Parameters;
            var parameterTypesByName = actionParameters.ToDictionary(p => p.Name, p => p.ParameterType);

            if (parameterTypesByName.TryGetValue("query", out var queryParameterType))
            {
                foreach (var prop in queryParameterType.GetProperties())
                {
                    parameterTypesByName[prop.Name] = prop.PropertyType;
                }
            }
            
            foreach (var parameter in operation.Parameters)
            {
                var parameterType = parameterTypesByName[parameter.Name];
                var isEntityIdType = EntityId.IsEntityIdType(parameterType);

                if (isEntityIdType)
                {
                    parameter.Example = new OpenApiString(EntityId.CreateExampleValue(parameterType));
                }
            }
        }
    }
}
