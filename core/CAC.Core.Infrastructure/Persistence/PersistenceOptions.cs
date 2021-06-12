namespace CAC.Core.Infrastructure.Persistence
{
    public sealed class PersistenceOptions
    {
        public const string ConfigKey = "Persistence";
        
        public FileSystemStoragePersistenceOptions FileSystemStorage { get; init; } = new();
    }
}
