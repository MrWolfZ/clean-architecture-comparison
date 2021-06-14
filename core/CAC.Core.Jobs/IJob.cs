using System.Threading;
using System.Threading.Tasks;

namespace CAC.Core.Jobs
{
    public interface IJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
