using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CAC.Core.Web
{
    public static class CoreErrorHandlingMiddleware
    {
        public static void UseCoreErrorHandling(
            this IApplicationBuilder app,
            IHostEnvironment environment,
            string apiRoutePrefix = "api",
            string errorHandlingPath = "/ui/home/error")
        {
            app.UseIfElse(IsApiRequest, ApiExceptionMiddleware, DefaultExceptionMiddleware);

            static void ApiExceptionMiddleware(IApplicationBuilder a) => a.UseProblemDetails();

            void DefaultExceptionMiddleware(IApplicationBuilder a)
            {
                if (environment.IsDevelopment() || environment.IsStaging())
                {
                    a.UseDeveloperExceptionPage();
                }
                else
                {
                    a.UseExceptionHandler(errorHandlingPath);
                }

                a.UseStatusCodePages();
            }

            bool IsApiRequest(HttpContext httpContext) => httpContext.Request.Path.StartsWithSegments($"/{apiRoutePrefix}", StringComparison.OrdinalIgnoreCase);
        }

        private static void UseIfElse(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> ifCase, Action<IApplicationBuilder> elseCase)
        {
            app.UseWhen(predicate, ifCase);
            app.UseWhen(ctx => !predicate(ctx), elseCase);
        }
    }
}
