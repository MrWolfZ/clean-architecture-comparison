using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CAC.Core.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJobTriggerService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JobTriggerOptions>()
                    .Bind(configuration.GetSection(JobTriggerOptions.ConfigKey))
                    .ValidateDataAnnotations();
            
            services.AddHostedService<JobTriggerService>();
        }
        
        public static void AddJob<TJob>(this IServiceCollection services)
            where TJob : class, IJob
        {
            services.AddTransient<IJob, TJob>();
        }
    }
}
