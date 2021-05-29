using System.Threading.Tasks;

namespace CAC.CQS.Domain
{
    public interface IQueryHandler<in TQuery, TResponse>
    {
        Task<TResponse> ExecuteQuery(TQuery taskListId);
    }
}
