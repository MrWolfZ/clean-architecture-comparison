using System;
using System.Runtime.Serialization;

// property Type should have that name to match the naming of ProblemDetails
#pragma warning disable CA1721

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Domain.Exceptions
{
    [Serializable]
    public class DomainEntityNotFoundException : DomainEntityException
    {
        public DomainEntityNotFoundException(EntityId entityId, string? message = null)
            : base(entityId, message ?? $"entity {entityId} does not exist")
        {
        }

        // ignore non-nullable field uninitialized for serialization constructor
#pragma warning disable CS8618
        protected DomainEntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore CS8618

        public override string Details { get; init; } = string.Empty;

        public override string Type => "https://cac.com/errors/domain-entity-not-found";
    }
}
