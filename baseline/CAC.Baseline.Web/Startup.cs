using System.Reflection;
using System.Runtime.CompilerServices;
using CAC.Baseline.Web.Persistence;
using CAC.Baseline.Web.Services;
using CAC.Core.Infrastructure;
using CAC.Core.Infrastructure.Serialization;
using CAC.Core.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.Baseline.UnitTests")]

namespace CAC.Baseline.Web
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

            services.AddPersistenceOptions(Configuration);

            services.AddTransient<ITaskListRepository, FileSystemTaskListRepository>();
            services.AddTransient<ITaskListEntryRepository, FileSystemTaskListEntryRepository>();
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
