using System.Runtime.CompilerServices;
using CAC.Core.Domain;
using Hellang.Middleware.ProblemDetails;
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

                setup.Map<DomainValidationException>(DomainValidationProblemDetails.New);
            });
        }
    }
}
