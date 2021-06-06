using System.Threading.Tasks;

namespace CAC.Baseline.Web.Services
{
    // in a real system this adapter would put the message on a message queue
    // for other systems to consume
    internal sealed class NullMessageQueueAdapter : IMessageQueueAdapter
    {
        public Task Send<T>(T message)
            where T : class
        {
            // nothing to do
            return Task.CompletedTask;
        }
    }
}
