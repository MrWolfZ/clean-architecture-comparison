using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain;

namespace CAC.Core.Application
{
    public interface IDomainEventPublisher
    {
        Task Publish(DomainEvent evt, params DomainEvent[] otherEvents);
        
        Task Publish(DomainEvent evt, CancellationToken cancellationToken, params DomainEvent[] otherEvents);

        Task Publish(IReadOnlyCollection<DomainEvent> events);

        Task Publish(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken);
    }
}
