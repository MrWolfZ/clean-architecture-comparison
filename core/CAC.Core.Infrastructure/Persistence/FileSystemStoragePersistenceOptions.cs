using System.ComponentModel.DataAnnotations;

namespace CAC.Core.Infrastructure.Persistence
{
    public sealed class FileSystemStoragePersistenceOptions
    {
        [Required]
        public string BaseDir { get; set; } = string.Empty;
    }
}
