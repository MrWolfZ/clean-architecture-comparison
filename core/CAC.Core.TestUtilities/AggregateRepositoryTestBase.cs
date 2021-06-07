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

        protected Mock<IDomainEventPublisher> DomainEventPublisherMock { get; } = new();

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

            aggregate = await Testee.Upsert(aggregate);

            var storedAggregate = await Testee.GetById(aggregate.Id);
            Assert.AreEqual(aggregate, storedAggregate);
        }

        [Test]
        public async Task Upsert_GivenExistingAggregate_StoresAggregate()
        {
            var existingAggregate = CreateAggregate();
            existingAggregate = await Testee.Upsert(existingAggregate);

            var updatedAggregate = UpdateAggregate(existingAggregate);
            updatedAggregate = await Testee.Upsert(updatedAggregate);

            var storedAggregate = await Testee.GetById(updatedAggregate.Id);
            Assert.AreEqual(updatedAggregate, storedAggregate);
        }

        [Test]
        public async Task Upsert_GivenNonExistingDeletedAggregate_DoesNotStoreAggregate()
        {
            var aggregate = CreateAggregate().MarkAsDeleted();

            aggregate = await Testee.Upsert(aggregate);

            var storedList = await Testee.GetById(aggregate.Id);
            Assert.IsNull(storedList);
        }

        [Test]
        public async Task Upsert_GivenExistingDeletedAggregate_DeletesAggregate()
        {
            var aggregate = CreateAggregate();
            aggregate = await Testee.Upsert(aggregate);

            aggregate = await Testee.Upsert(aggregate.MarkAsDeleted());

            var storedList = await Testee.GetById(aggregate.Id);
            Assert.IsNull(storedList);
        }

        [Test]
        public async Task Upsert_GivenAggregateWithEvents_PublishesEvents()
        {
            var originalAggregate = CreateAggregate();
            originalAggregate = await Testee.Upsert(originalAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(originalAggregate.DomainEvents));

            var updatedAggregate = UpdateAggregate(originalAggregate);
            updatedAggregate = await Testee.Upsert(updatedAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(updatedAggregate.DomainEvents));
        }

        [Test]
        public async Task Upsert_GivenDeletedAggregateWithEvents_PublishesEvents()
        {
            var originalAggregate = CreateAggregate();
            originalAggregate = await Testee.Upsert(originalAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(originalAggregate.DomainEvents));

            var updatedAggregate = UpdateAggregate(originalAggregate).MarkAsDeleted();
            updatedAggregate = await Testee.Upsert(updatedAggregate);
            
            DomainEventPublisherMock.Verify(p => p.Publish(updatedAggregate.DomainEvents));
        }

        [Test]
        public async Task Upsert_GivenNonExistingAggregateWithEvents_ReturnsAggregateWithoutEvents()
        {
            var aggregate = CreateAggregate();
            aggregate = await Testee.Upsert(aggregate);
            
            Assert.IsEmpty(aggregate.DomainEvents);
        }

        [Test]
        public async Task Upsert_GivenExistingAggregateWithEvents_ReturnsAggregateWithoutEvents()
        {
            var existingAggregate = CreateAggregate();
            existingAggregate = await Testee.Upsert(existingAggregate);

            var updatedAggregate = UpdateAggregate(existingAggregate);
            updatedAggregate = await Testee.Upsert(updatedAggregate);
            
            Assert.IsEmpty(updatedAggregate.DomainEvents);
        }

        [Test]
        public async Task Upsert_GivenDeletedAggregateWithEvents_ReturnsAggregateWithoutEvents()
        {
            var originalAggregate = CreateAggregate();
            originalAggregate = await Testee.Upsert(originalAggregate);
            
            var updatedAggregate = UpdateAggregate(originalAggregate).MarkAsDeleted();
            updatedAggregate = await Testee.Upsert(updatedAggregate);
            
            Assert.IsEmpty(updatedAggregate.DomainEvents);
        }

        protected abstract TAggregate CreateAggregate();

        protected abstract TAggregate UpdateAggregate(TAggregate aggregate);
    }
}
