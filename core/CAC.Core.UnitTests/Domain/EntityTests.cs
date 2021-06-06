using System;
using CAC.Core.Domain;
using NUnit.Framework;

// we want to mirror entity id types, which do not have public constructors 
#pragma warning disable S3453

namespace CAC.Core.UnitTests.Domain
{
    public sealed class EntityTests
    {
        [Test]
        public void EntityId_Parse_GivenValidStringAndType_CreatesInstanceOfType()
        {
            var value = $"{nameof(TestEntity)}-1";
            var id = EntityId.Parse<TestEntityId>(value);

            Assert.NotNull(id);
            Assert.IsInstanceOf<TestEntityId>(id);
            Assert.AreEqual(value, id?.Value);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(nameof(TestEntity) + "-a")]
        [TestCase(nameof(TestEntity) + "-1a")]
        [TestCase("Foo-1")]
        public void EntityId_Parse_GivenInvalidString_ThrowsArgumentException(string value)
        {
            _ = Assert.Throws<ArgumentException>(() => EntityId.Parse<TestEntityId>(value));
        }
        
        // ReSharper disable ClassNeverInstantiated.Local
        private sealed record TestEntity : Entity<TestEntity, TestEntityId>
        {
            // ReSharper disable once UnusedMember.Local
            public TestEntity(TestEntityId id)
                : base(id)
            {
            }
        }

        private sealed record TestEntityId : EntityId<TestEntity>
        {
            // ReSharper disable once UnusedMember.Local
            // constructor must be private to mirror behavior of real entity id types
            private TestEntityId(long numericId)
                : base(numericId)
            {
            }
        }
    }
}
