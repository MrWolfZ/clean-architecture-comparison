using System.Threading.Tasks;
using CAC.Baseline.Web.Model;

namespace CAC.Baseline.Web.Data
{
    public interface IUserRepository
    {
        public Task<User?> GetById(long id);
    }
}
