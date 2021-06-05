namespace CAC.Baseline.Web.Model
{
    public sealed class User
    {
        public User(long id, string name, bool isPremium)
        {
            Id = id;
            Name = name;
            IsPremium = isPremium;
        }

        public long Id { get; }

        public string Name { get; }

        public bool IsPremium { get; }
    }
}
