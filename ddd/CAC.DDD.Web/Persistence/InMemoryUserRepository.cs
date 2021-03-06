using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Persistence
{
    internal sealed class InMemoryUserRepository : IUserRepository
    {
        // for simplicity's sake, we simply hard-code two users instead of coding full CRUD support
        private static readonly IDictionary<UserId, User> Users = new Dictionary<UserId, User>
        {
            { 1, User.FromRawData(1, "premium", true) },
            { 2, User.FromRawData(2, "non-premium", false) },
        };

        public Task<User?> GetById(UserId id) => Task.FromResult(Users.TryGetValue(id, out var user) ? user : null);
    }
}
