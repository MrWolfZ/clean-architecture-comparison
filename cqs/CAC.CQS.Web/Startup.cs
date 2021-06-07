using System.Reflection;
using CAC.Core.Infrastructure.Serialization;
using CAC.Core.Web;
using CAC.CQS.Domain;
using CAC.CQS.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CAC.CQS.Web
{
    public sealed class Startup
    {
        private const string ApiVersion = "v1";

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        private static string? AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;

        private IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(c => c.UseApiPrefix()).AddJsonOptions(setup => setup.JsonSerializerOptions.AddCoreConverters());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersion, new OpenApiInfo { Title = AssemblyName, Version = ApiVersion });
                c.ConfigureCore();
            });

            services.AddCoreWeb(Environment);

            services.AddDomain();
            services.AddInfrastructure();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCoreErrorHandling(Environment);

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", $"{AssemblyName} {ApiVersion}"));

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
