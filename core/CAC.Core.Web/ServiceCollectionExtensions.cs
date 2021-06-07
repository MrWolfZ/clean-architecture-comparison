using System.Runtime.CompilerServices;
using CAC.Core.Domain.Exceptions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.Core.Web
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCoreWeb(this IServiceCollection services, IHostEnvironment environment)
        {
            services.AddProblemDetails(setup =>
            {
                setup.IncludeExceptionDetails = (_, _) => environment.IsDevelopment() || environment.IsStaging();

                setup.Map<DomainEntityNotFoundException>(ex => new DomainExceptionProblemDetails(ex, StatusCodes.Status404NotFound));
                setup.Map<DomainEntityException>(ex => new DomainExceptionProblemDetails(ex, StatusCodes.Status409Conflict));
            });
        }
    }
}
