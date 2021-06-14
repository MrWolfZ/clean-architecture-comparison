using System.Threading.Tasks;

namespace CAC.CQS.MediatR.Application
{
    public interface IMessageQueueAdapter
    {
        Task Send<T>(T message)
            where T : class;
    }
}