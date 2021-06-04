namespace CAC.Core.Infrastructure
{
    public sealed class PersistenceOptions
    {
        public FileSystemStoragePersistenceOptions FileSystemStorage { get; init; } = new FileSystemStoragePersistenceOptions();
    }
}
