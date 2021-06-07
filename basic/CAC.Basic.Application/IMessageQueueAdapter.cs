using System.Threading.Tasks;

namespace CAC.Basic.Application
{
    public interface IMessageQueueAdapter
    {
        Task Send<T>(T message)
            where T : class;
    }
}
