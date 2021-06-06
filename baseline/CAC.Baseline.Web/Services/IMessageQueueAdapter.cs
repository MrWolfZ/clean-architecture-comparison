using System.Threading.Tasks;

namespace CAC.Baseline.Web.Services
{
    public interface IMessageQueueAdapter
    {
        Task Send<T>(T message)
            where T : class;
    }
}
