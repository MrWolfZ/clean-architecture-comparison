using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.CQS.MediatR.Domain.UserAggregate;

namespace CAC.CQS.MediatR.Application.Users
{
    public interface IUserRepository
    {
        public Task<User?> GetById(UserId id);

        public Task<IReadOnlyCollection<User>> GetPremiumUsers();
    }
}