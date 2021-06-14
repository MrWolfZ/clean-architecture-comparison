using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.MediatR.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.MediatR.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatingBehavior<,>));

            services.AddTransient<IDomainEventPublisher, MediatRAdapterPublisher>();

            typeof(TaskListId).Assembly.AddEntityIdTypeConverterAttributes();
        }

        private sealed class MediatRAdapterPublisher : IDomainEventPublisher
        {
            private readonly IMediator mediator;

            public MediatRAdapterPublisher(IMediator mediator)
            {
                this.mediator = mediator;
            }

            public Task Publish(DomainEvent evt, params DomainEvent[] otherEvents)
                => Publish(new[] { evt }.Concat(otherEvents).ToList());

            public async Task Publish(IReadOnlyCollection<DomainEvent> events)
            {
                foreach (var notification in events.OfType<INotification>())
                {
                    await mediator.Publish(notification);
                }
            }
        }
    }
}
