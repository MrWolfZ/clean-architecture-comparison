using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CAC.Core.Web
{
    public static class CoreErrorHandlingMiddleware
    {
        public static IApplicationBuilder UseCoreErrorHandling(
            this IApplicationBuilder app,
            IHostEnvironment environment,
            string apiRoutePrefix = "api",
            string errorHandlingPath = "/ui/home/error")
        {
            return app.UseIfElse(IsApiRequest, ApiExceptionMiddleware, DefaultExceptionMiddleware);

            static void ApiExceptionMiddleware(IApplicationBuilder a) => a.UseProblemDetails();

            void DefaultExceptionMiddleware(IApplicationBuilder a)
            {
                if (environment.IsDevelopment() || environment.IsStaging())
                {
                    _ = a.UseDeveloperExceptionPage();
                }
                else
                {
                    _ = a.UseExceptionHandler(errorHandlingPath);
                }

                _ = a.UseStatusCodePages();
            }

            bool IsApiRequest(HttpContext httpContext) => httpContext.Request.Path.StartsWithSegments($"/{apiRoutePrefix}", StringComparison.OrdinalIgnoreCase);
        }

        private static IApplicationBuilder UseIfElse(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> ifCase, Action<IApplicationBuilder> elseCase)
        {
            return app.UseWhen(predicate, ifCase)
                      .UseWhen(ctx => !predicate(ctx), elseCase);
        }
    }
}
