using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.CQS.Domain.UserAggregate;

namespace CAC.CQS.Application.Users
{
    public interface IUserRepository
    {
        public Task<User?> GetById(UserId id);

        public Task<IReadOnlyCollection<User>> GetPremiumUsers();
    }
}