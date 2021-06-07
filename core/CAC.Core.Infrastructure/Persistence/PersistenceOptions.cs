namespace CAC.Core.Infrastructure.Persistence
{
    public sealed class PersistenceOptions
    {
        public FileSystemStoragePersistenceOptions FileSystemStorage { get; init; } = new();
    }
}
