using System.Threading.Tasks;
using CAC.Basic.Domain.UserAggregate;

namespace CAC.Basic.Application.Users
{
    public interface IUserRepository
    {
        public Task<User?> GetById(UserId id);
    }
}
