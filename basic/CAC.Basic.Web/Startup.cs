using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Basic.Application;
using CAC.Basic.Domain.TaskListAggregate;
using CAC.Basic.Infrastructure;
using CAC.Core.Domain;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.Infrastructure.Serialization;
using CAC.Core.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Basic.UnitTests")]

namespace CAC.Basic.Web
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
                c.SwaggerDoc(ApiVersion, new() { Title = AssemblyName, Version = ApiVersion });
                c.ConfigureCore();
            });

            services.AddCoreWeb(Environment);
            
            typeof(TaskListId).Assembly.AddEntityIdTypeConverterAttributes();

            services.AddApplication();
            services.AddInfrastructure();

            services.AddOptions<PersistenceOptions>("Persistence");
            services.ConfigureOptions<FileSystemStoragePersistenceOptionsDevelopmentConfiguration>();
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
