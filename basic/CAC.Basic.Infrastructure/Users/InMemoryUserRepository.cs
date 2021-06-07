using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Basic.Application.Users;
using CAC.Basic.Domain.UserAggregate;

namespace CAC.Basic.Infrastructure.Users
{
    internal sealed class InMemoryUserRepository : IUserRepository
    {
        // for simplicity's sake, we simply hard-code two users instead of coding full CRUD support
        private static readonly IDictionary<UserId, User> Users = new Dictionary<UserId, User>
        {
            { 1, User.New(1, "premium", true) },
            { 2, User.New(2, "non-premium", false) },
        };

        public Task<User?> GetById(UserId id) => Task.FromResult(Users.TryGetValue(id, out var user) ? user : null);
    }
}
