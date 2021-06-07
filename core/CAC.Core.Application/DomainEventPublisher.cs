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
        private readonly ConcurrentDictionary<Type, Type> handlerTypeByEventType = new ConcurrentDictionary<Type, Type>();
        private readonly ConcurrentDictionary<(IDomainEventHandler, Type), Func<DomainEvent, Task>> publishFunctions = new ConcurrentDictionary<(IDomainEventHandler, Type), Func<DomainEvent, Task>>();

        public DomainEventPublisher(IEnumerable<IDomainEventHandler> eventHandlers) => this.eventHandlers = eventHandlers.ToList();

        public Task Publish(DomainEvent evt, params DomainEvent[] otherEvents) => Publish(new[] { evt }.Concat(otherEvents).ToList());

        public async Task Publish(IReadOnlyCollection<DomainEvent> events)
        {
            foreach (var evt in events)
            {
                await Publish(evt);
            }
        }

        private async Task Publish(DomainEvent evt)
        {
            foreach (var handler in GetRelevantHandlers(evt))
            {
                await Publish(handler, evt);
            }
        }

        private async Task Publish(IDomainEventHandler eventHandler, DomainEvent evt)
        {
            var publishFunction = publishFunctions.GetOrAdd((eventHandler, evt.GetType()), t => CreatePublishFunction(t.Item1, t.Item2));
            await publishFunction(evt);
        }

        private IEnumerable<IDomainEventHandler> GetRelevantHandlers(DomainEvent evt)
        {
            var handlerType = handlerTypeByEventType.GetOrAdd(evt.GetType(), t => typeof(IDomainEventHandler<>).MakeGenericType(t));
            return eventHandlers.Where(h => h.GetType().IsAssignableTo(handlerType));
        }

        private static Func<DomainEvent, Task> CreatePublishFunction(IDomainEventHandler eventHandler, Type eventType)
        {
            var parameterExpression = Expression.Parameter(typeof(DomainEvent));
            var castedParameter = Expression.Convert(parameterExpression, eventType);
            var callExpr = Expression.Call(Expression.Constant(eventHandler), nameof(IDomainEventHandler<DomainEvent>.Handle), null, castedParameter);
            var lambda = Expression.Lambda(callExpr, parameterExpression);
            var compiled = lambda.Compile();
            return (Func<DomainEvent, Task>)compiled;
        }
    }
}
