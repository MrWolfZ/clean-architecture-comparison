using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Core.Domain;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.Infrastructure.Serialization;
using CAC.Core.Web;
using CAC.DDD.Web.Persistence;
using CAC.DDD.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

[assembly: InternalsVisibleTo("CAC.DDD.UnitTests")]

namespace CAC.DDD.Web
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
            
            Assembly.GetExecutingAssembly().AddEntityIdTypeConverterAttributes();

            services.AddOptions<PersistenceOptions>("Persistence");
            services.ConfigureOptions<FileSystemStoragePersistenceOptionsDevelopmentConfiguration>();

            services.AddTransient<ITaskListRepository, FileSystemTaskListRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<ITaskListStatisticsRepository, InMemoryTaskListStatisticsRepository>();
            
            services.AddTransient<ITaskListStatisticsService, TaskListStatisticsService>();
            services.AddTransient<ITaskListNotificationService, TaskListNotificationService>();
            
            services.AddTransient<IMessageQueueAdapter, NullMessageQueueAdapter>();
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
