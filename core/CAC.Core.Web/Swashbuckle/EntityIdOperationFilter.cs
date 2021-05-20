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
            var parameterDescriptorsByName = context.ApiDescription.ActionDescriptor.Parameters.ToDictionary(p => p.Name);
            foreach (var parameter in operation.Parameters)
            {
                var parameterDescriptor = parameterDescriptorsByName[parameter.Name];
                var isEntityIdType = EntityId.IsEntityIdType(parameterDescriptor.ParameterType);

                if (isEntityIdType)
                {
                    parameter.Example = new OpenApiString(EntityId.CreateExampleValue(parameterDescriptor.ParameterType));
                }
            }
        }
    }
}
