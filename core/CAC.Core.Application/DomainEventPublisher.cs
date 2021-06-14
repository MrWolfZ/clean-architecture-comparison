using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CAC.Core.Domain;

namespace CAC.Core.Application
{
    internal sealed class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IReadOnlyCollection<IDomainEventHandler> eventHandlers;
        private readonly ConcurrentDictionary<Type, IReadOnlyCollection<IDomainEventHandler>> handlersByEventType = new();
        private readonly ConcurrentDictionary<Type, Func<DomainEvent, CancellationToken, Task>> publishFunctions = new();

        public DomainEventPublisher(IEnumerable<IDomainEventHandler> eventHandlers)
        {
            this.eventHandlers = eventHandlers.ToList();
        }

        public Task Publish(DomainEvent evt, params DomainEvent[] otherEvents) => Publish(evt, CancellationToken.None, otherEvents);

        public Task Publish(DomainEvent evt, CancellationToken cancellationToken, params DomainEvent[] otherEvents)
            => Publish(new[] { evt }.Concat(otherEvents).ToList(), cancellationToken);

        public Task Publish(IReadOnlyCollection<DomainEvent> events) => Publish(events, CancellationToken.None);

        public async Task Publish(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken)
        {
            foreach (var evt in events)
            {
                try
                {
                    await PublishSingle(evt, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // if publishing is cancelled we continue iterating over all handlers
                    // to give each one a chance to react to the cancellation as desired
                }
            }
            
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task PublishSingle(DomainEvent evt, CancellationToken cancellationToken)
        {
            var publishFn = publishFunctions.GetOrAdd(evt.GetType(), CreatePublishFunction);
            await publishFn(evt, cancellationToken);
        }

        private async Task PublishSingleGeneric<TDomainEvent>(TDomainEvent evt, CancellationToken cancellationToken)
            where TDomainEvent : DomainEvent
        {
            foreach (var handler in GetRelevantHandlers<TDomainEvent>())
            {
                await handler.Handle(evt, cancellationToken);
            }
        }

        private IEnumerable<IDomainEventHandler<TDomainEvent>> GetRelevantHandlers<TDomainEvent>()
            where TDomainEvent : DomainEvent
        {
            var handlers = handlersByEventType.GetOrAdd(typeof(TDomainEvent), _ => eventHandlers.Where(h => h is IDomainEventHandler<TDomainEvent>).ToList());
            return handlers.Cast<IDomainEventHandler<TDomainEvent>>();
        }

        private Func<DomainEvent, CancellationToken, Task> CreatePublishFunction(Type eventType)
        {
            var parameterExpression = Expression.Parameter(typeof(DomainEvent));
            var cancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken));
            var castedParameter = Expression.Convert(parameterExpression, eventType);
            var callExpr = Expression.Call(Expression.Constant(this), nameof(PublishSingleGeneric), new[] { eventType }, castedParameter, cancellationTokenParameterExpression);
            var lambda = Expression.Lambda(callExpr, parameterExpression, cancellationTokenParameterExpression);
            var compiled = lambda.Compile();
            return (Func<DomainEvent, CancellationToken, Task>)compiled;
        }
    }
}
