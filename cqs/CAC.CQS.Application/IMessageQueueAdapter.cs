using System.Threading.Tasks;

namespace CAC.CQS.Application
{
    public interface IMessageQueueAdapter
    {
        Task Send<T>(T message)
            where T : class;
    }
}