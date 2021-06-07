using System.Threading.Tasks;
using CAC.Core.Domain;

// EventHandler is a fitting suffix in this case
#pragma warning disable CA1711

// empty interface used as marker interface for other operations
#pragma warning disable CA1040

namespace CAC.Core.Application
{
    public interface IDomainEventHandler<in TEvent> : IDomainEventHandler
        where TEvent : DomainEvent
    {
        Task Handle(TEvent evt);
    }

    public interface IDomainEventHandler
    {
    }
}
