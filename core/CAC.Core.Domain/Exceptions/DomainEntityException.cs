using System;
using System.Runtime.Serialization;

// property Type should have that name to match the naming of ProblemDetails
#pragma warning disable CA1721

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Domain.Exceptions
{
    [Serializable]
    public abstract class DomainEntityException : Exception
    {
        protected DomainEntityException(EntityId entityId, string message)
            : base(message)
        {
            EntityId = entityId;
        }

        // ignore non-nullable field uninitialized for serialization constructor
#pragma warning disable CS8618
        protected DomainEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore CS8618

        public abstract string Details { get; init; }

        public abstract string Type { get; }

        public EntityId EntityId { get; }
    }
}
