using System.Threading.Tasks;
using CAC.Core.Domain;

namespace CAC.Core.Application
{
    public interface IAggregateRepository<TAggregate, TId>
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId : EntityId<TAggregate>
    {
        public Task<TId> GenerateId();

        public Task<TAggregate> Upsert(TAggregate aggregate);

        public Task<TAggregate?> GetById(TId id);
    }
}
