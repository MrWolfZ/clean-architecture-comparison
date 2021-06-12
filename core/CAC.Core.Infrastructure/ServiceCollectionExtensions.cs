using CAC.Core.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CAC.Core.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPersistenceOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<PersistenceOptions>()
                    .Bind(configuration.GetSection(PersistenceOptions.ConfigKey))
                    .ValidateDataAnnotations();

            services.ConfigureOptions<FileSystemStoragePersistenceOptionsDevelopmentConfiguration>();
        }
    }
}
