using System.Collections.Generic;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.Core.Domain;
using Moq;
using NUnit.Framework;

#pragma warning disable CA1707

namespace CAC.Core.TestUtilities
{
    public abstract class AggregateRepositoryTestBase<TAggregate, TId>
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId : EntityId<TAggregate>
    {
        protected abstract IAggregateRepository<TAggregate, TId> Testee { get; }

        protected Mock<IDomainEventPublisher> DomainEventPublisherMock { get; } = new Mock<IDomainEventPublisher>();

        protected IDomainEventPublisher DomainEventPublisher => DomainEventPublisherMock.Object;

        [Test]
        public async Task GenerateId_ReturnsNewIdOnEachCall()
        {
            var generatedIds = new HashSet<TId>();

            for (var i = 0; i < 100; i += 1)
            {
                var id = await Testee.GenerateId();
                Assert.IsFalse(generatedIds.Contains(id));
                _ = generatedIds.Add(id);
            }
        }

        [Test]
        public async Task Upsert_GivenNonExistingAggregate_StoresAggregate()
        {
            var aggregate = CreateAggregate();

            await Testee.Upsert(aggregate);

            var storedAggregate = await Testee.GetById(aggregate.Id);
            Assert.AreEqual(aggregate, storedAggregate);
        }

        [Test]
        public async Task Upsert_GivenExistingAggregate_StoresAggregate()
        {
            var existingAggregate = CreateAggregate();
            await Testee.Upsert(existingAggregate);

            var aggregate = UpdateAggregate(existingAggregate);
            await Testee.Upsert(aggregate);

            var storedAggregate = await Testee.GetById(aggregate.Id);
            Assert.AreEqual(aggregate.WithoutEvents(), storedAggregate);
        }

        [Test]
        public async Task Upsert_GivenNonExistingDeletedAggregate_DoesNotStoreAggregate()
        {
            var list = CreateAggregate().MarkAsDeleted();

            await Testee.Upsert(list);

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNull(storedList);
        }

        [Test]
        public async Task Upsert_GivenExistingDeletedAggregate_DeletesAggregate()
        {
            var list = CreateAggregate();
            await Testee.Upsert(list);

            await Testee.Upsert(list.MarkAsDeleted());

            var storedList = await Testee.GetById(list.Id);
            Assert.IsNull(storedList);
        }

        [Test]
        public async Task Upsert_GivenAggregateWithDomainEvents_PublishesEvents()
        {
            var originalAggregate = CreateAggregate();
            await Testee.Upsert(originalAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(originalAggregate.DomainEvents));

            var updatedAggregate = UpdateAggregate(originalAggregate.WithoutEvents());
            await Testee.Upsert(updatedAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(updatedAggregate.DomainEvents));
        }

        [Test]
        public async Task Upsert_GivenDeletedAggregateWithDomainEvents_PublishesEvents()
        {
            var originalAggregate = CreateAggregate();
            await Testee.Upsert(originalAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(originalAggregate.DomainEvents));

            var updatedAggregate = UpdateAggregate(originalAggregate.WithoutEvents()).MarkAsDeleted();
            await Testee.Upsert(updatedAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(updatedAggregate.DomainEvents));
        }

        protected abstract TAggregate CreateAggregate();

        protected abstract TAggregate UpdateAggregate(TAggregate aggregate);
    }
}
