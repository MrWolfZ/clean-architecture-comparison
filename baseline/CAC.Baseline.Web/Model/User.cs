using System;

namespace CAC.Baseline.Web.Model
{
    public sealed class User
    {
        public User(long id, string name, bool isPremium)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id must be a positive integer", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name must be a non-empty non-whitespace string", nameof(name));
            }

            Id = id;
            Name = name;
            IsPremium = isPremium;
        }

        public long Id { get; }

        public string Name { get; }

        public bool IsPremium { get; }
    }
}
