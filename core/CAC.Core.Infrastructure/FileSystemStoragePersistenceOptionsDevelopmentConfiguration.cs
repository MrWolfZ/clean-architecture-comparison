using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CAC.Core.Infrastructure
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

            while (currentDir != null && !Directory.Exists(Path.Join(currentDir, ".git")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            if (currentDir == null)
            {
                throw new InvalidOperationException($"could not find repository root relative to dir '{AppContext.BaseDirectory}'");
            }

            options.BaseDir = Path.Join(currentDir, ".data");

            Directory.CreateDirectory(options.BaseDir);
        }
    }
}
