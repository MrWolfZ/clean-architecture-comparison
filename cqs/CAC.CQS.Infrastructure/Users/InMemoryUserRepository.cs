using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAC.CQS.Application.Users;
using CAC.CQS.Domain.UserAggregate;

namespace CAC.CQS.Infrastructure.Users
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

        public async Task<IReadOnlyCollection<User>> GetPremiumUsers()
        {
            await Task.Yield();
            return Users.Values.Where(u => u.IsPremium).ToList();
        }
    }
}