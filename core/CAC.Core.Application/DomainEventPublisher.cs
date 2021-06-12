using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CAC.Core.Domain;

namespace CAC.Core.Application
{
    internal sealed class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IReadOnlyCollection<IDomainEventHandler> eventHandlers;
        private readonly ConcurrentDictionary<Type, IReadOnlyCollection<IDomainEventHandler>> handlersByEventType = new();
        private readonly ConcurrentDictionary<Type, Func<DomainEvent, Task>> publishFunctions = new();

        public DomainEventPublisher(IEnumerable<IDomainEventHandler> eventHandlers)
        {
            this.eventHandlers = eventHandlers.ToList();
        }

        public Task Publish(DomainEvent evt, params DomainEvent[] otherEvents)
            => Publish(new[] { evt }.Concat(otherEvents).ToList());

        public async Task Publish(IReadOnlyCollection<DomainEvent> events)
        {
            foreach (var evt in events)
            {
                await PublishSingle(evt);
            }
        }

        private async Task PublishSingle(DomainEvent evt)
        {
            var publishFn = publishFunctions.GetOrAdd(evt.GetType(), CreatePublishFunction);
            await publishFn(evt);
        }

        private async Task PublishSingleGeneric<TDomainEvent>(TDomainEvent evt)
            where TDomainEvent : DomainEvent
        {
            foreach (var handler in GetRelevantHandlers<TDomainEvent>())
            {
                await handler.Handle(evt);
            }
        }

        private IEnumerable<IDomainEventHandler<TDomainEvent>> GetRelevantHandlers<TDomainEvent>()
            where TDomainEvent : DomainEvent
        {
            var handlers = handlersByEventType.GetOrAdd(typeof(TDomainEvent), _ => eventHandlers.Where(h => h is IDomainEventHandler<TDomainEvent>).ToList());
            return handlers.Cast<IDomainEventHandler<TDomainEvent>>();
        }

        private Func<DomainEvent, Task> CreatePublishFunction(Type eventType)
        {
            var parameterExpression = Expression.Parameter(typeof(DomainEvent));
            var castedParameter = Expression.Convert(parameterExpression, eventType);
            var callExpr = Expression.Call(Expression.Constant(this), nameof(PublishSingleGeneric), new[] { eventType }, castedParameter);
            var lambda = Expression.Lambda(callExpr, parameterExpression);
            var compiled = lambda.Compile();
            return (Func<DomainEvent, Task>)compiled;
        }
    }
}
