using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Persistence
{
    public interface IUserRepository
    {
        public Task<User?> GetById(long id);
    }
}
