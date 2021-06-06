using System.Threading.Tasks;
using CAC.DDD.Web.Domain.UserAggregate;

namespace CAC.DDD.Web.Persistence
{
    public interface IUserRepository
    {
        public Task<User?> GetById(UserId id);
    }
}
