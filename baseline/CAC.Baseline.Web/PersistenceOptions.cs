namespace CAC.Baseline.Web
{
    public sealed class PersistenceOptions
    {
        public FileSystemStoragePersistenceOptions FileSystemStorage { get; init; } = new FileSystemStoragePersistenceOptions();
    }
}
