using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CAC.Core.Infrastructure.Persistence
{
    public sealed class FileSystemStoragePersistenceOptionsDevelopmentConfiguration : IConfigureOptions<FileSystemStoragePersistenceOptions>
    {
        private readonly IHostEnvironment environment;

        public FileSystemStoragePersistenceOptionsDevelopmentConfiguration(IHostEnvironment environment)
        {
            this.environment = environment;
        }

        public void Configure(FileSystemStoragePersistenceOptions options)
        {
            if (!environment.IsDevelopment())
            {
                return;
            }

            SetBaseDir(options);
        }

        private static void SetBaseDir(FileSystemStoragePersistenceOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.BaseDir))
            {
                return;
            }
            
            var currentDir = AppContext.BaseDirectory;

            while (currentDir != null && !File.Exists(Path.Join(currentDir, "Properties", "launchSettings.json")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            if (currentDir == null)
            {
                throw new InvalidOperationException($"could not find app base dir relative to dir '{AppContext.BaseDirectory}'");
            }
            
            options.BaseDir = Path.Join(Directory.GetParent(currentDir)?.FullName, ".data");

            _ = Directory.CreateDirectory(options.BaseDir);
        }
    }
}
