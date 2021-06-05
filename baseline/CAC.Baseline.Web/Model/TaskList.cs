namespace CAC.Baseline.Web.Model
{
    public sealed class TaskList
    {
        public TaskList(long id, long ownerId, string name)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
        }

        public long Id { get; }

        public long OwnerId { get; }

        public string Name { get; }
    }
}
