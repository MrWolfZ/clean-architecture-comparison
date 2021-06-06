using System.Threading.Tasks;

namespace CAC.DDD.Web.Services
{
    public interface IMessageQueueAdapter
    {
        Task Send<T>(T message)
            where T : class;
    }
}
