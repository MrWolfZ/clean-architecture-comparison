using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAC.Core.Web.Swashbuckle
{
    internal sealed class CommonCommonResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            AddSuccessResponse(operation, context);
            AddBadRequestResponse(operation, context);
            AddConflictResponse(operation, context);
            AdjustNotFoundResponse(operation, context);
        }

        private static void AddSuccessResponse(OpenApiOperation operation, OperationFilterContext context)
        {
            var operationContainsSuccessResponse = operation.Responses.Keys.Any(r => r.StartsWith("2"));

            if (operationContainsSuccessResponse)
            {
                return;
            }

            var responseType = GetResponseType();

            var operationHasResponseContent = responseType != typeof(void);

            var statusCode = operationHasResponseContent ? HttpStatusCode.OK : HttpStatusCode.NoContent;
            var key = $"{(int)statusCode}";

            var data = new OpenApiResponse
            {
                Description = statusCode.ToString(),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [MediaTypeNames.Application.Json] = GetResponseMediaType(),
                },
            };

            operation.Responses.Add(key, data);

            Type GetResponseType()
            {
                var returnType = context.MethodInfo.ReturnType;

                if (returnType == typeof(Task))
                {
                    return typeof(void);
                }

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    returnType = returnType.GetGenericArguments().Single();
                }

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                {
                    return returnType.GetGenericArguments().Single();
                }

                return returnType == typeof(IActionResult) ? typeof(void) : returnType;
            }

            OpenApiMediaType GetResponseMediaType()
            {
                if (!operationHasResponseContent)
                {
                    return new OpenApiMediaType();
                }

                var schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
                return new OpenApiMediaType { Schema = schema };
            }
        }

        private static void AddBadRequestResponse(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!operation.Parameters.Any() && operation.RequestBody is null)
            {
                return;
            }

            var badRequestKey = $"{(int)HttpStatusCode.BadRequest}";

            if (operation.Responses.ContainsKey(badRequestKey))
            {
                return;
            }

            var data = new OpenApiResponse
            {
                Description = HttpStatusCode.BadRequest.ToString(),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [MediaTypeNames.Application.Json] = GetProblemDetailsMediaType(context),
                },
            };

            operation.Responses.Add(badRequestKey, data);
        }

        private static void AddConflictResponse(OpenApiOperation operation, OperationFilterContext context)
        {
            var conflictKey = $"{(int)HttpStatusCode.Conflict}";
            var httpMethodsWithPotentialConflicts = new[] { HttpMethod.Post, HttpMethod.Put }.Select(m => m.Method).ToList();
            var methodCanHaveConflict = httpMethodsWithPotentialConflicts.Contains(context.ApiDescription.HttpMethod ?? string.Empty);

            if (!methodCanHaveConflict || operation.Responses.ContainsKey(conflictKey))
            {
                return;
            }

            var data = new OpenApiResponse
            {
                Description = HttpStatusCode.Conflict.ToString(),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [MediaTypeNames.Application.Json] = GetProblemDetailsMediaType(context),
                },
            };

            operation.Responses.Add(conflictKey, data);
        }

        private static void AdjustNotFoundResponse(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!operation.Responses.TryGetValue($"{(int)HttpStatusCode.NotFound}", out var notFoundResponse))
            {
                return;
            }

            notFoundResponse.Content.Clear();
            notFoundResponse.Content.Add(MediaTypeNames.Application.Json, GetProblemDetailsMediaType(context));
        }

        private static OpenApiMediaType GetProblemDetailsMediaType(OperationFilterContext context)
        {
            var problemDetailsSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);
            return new OpenApiMediaType { Schema = problemDetailsSchema };
        }
    }
}
