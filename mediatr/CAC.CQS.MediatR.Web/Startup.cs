using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Core.Infrastructure.Serialization;
using CAC.Core.Web;
using CAC.CQS.MediatR.Application;
using CAC.CQS.MediatR.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CAC.CQS.MediatR.UnitTests")]

namespace CAC.CQS.MediatR.Web
{
    public sealed class Startup
    {
        private const string ApiVersion = "v1";

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        private static string? AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;

        private IConfiguration Configuration { get; }

        private IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(c => c.UseApiPrefix()).AddJsonOptions(setup => setup.JsonSerializerOptions.AddCoreConverters());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersion, new() { Title = AssemblyName, Version = ApiVersion });
                c.ConfigureCore();
            });

            services.AddCoreWeb(Environment);

            services.AddApplication();
            services.AddInfrastructure(Configuration);

            services.AddTransient(p => p.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);
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
