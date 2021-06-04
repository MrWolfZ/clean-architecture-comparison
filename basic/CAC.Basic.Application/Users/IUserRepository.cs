using System.Threading.Tasks;
using CAC.Basic.Domain.Users;

namespace CAC.Basic.Application.Users
{
    public interface IUserRepository
    {
        public Task<User?> GetById(long id);
    }
}
