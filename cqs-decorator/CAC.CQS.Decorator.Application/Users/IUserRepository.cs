using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.CQS.Decorator.Domain.UserAggregate;

namespace CAC.CQS.Decorator.Application.Users
{
    public interface IUserRepository
    {
        public Task<User?> GetById(UserId id);

        public Task<IReadOnlyCollection<User>> GetPremiumUsers();
    }
}