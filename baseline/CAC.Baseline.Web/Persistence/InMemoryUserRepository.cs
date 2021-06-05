using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    internal sealed class InMemoryUserRepository : IUserRepository
    {
        // for simplicity's sake, we simply hard-code two users instead of coding full CRUD support
        private static readonly IDictionary<long, User> Users = new Dictionary<long, User>
        {
            { 1, new User(1, "premium", true) },
            { 2, new User(2, "non-premium", false) },
        };

        public Task<User?> GetById(long id) => Task.FromResult(Users.TryGetValue(id, out var user) ? user : null);
    }
}
