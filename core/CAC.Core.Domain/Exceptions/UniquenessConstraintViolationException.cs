using System;
using System.Runtime.Serialization;

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Domain.Exceptions
{
    [Serializable]
    public sealed class UniquenessConstraintViolationException : DomainEntityException
    {
        public UniquenessConstraintViolationException(EntityId entityId, string propertyName, object conflictingValue)
            : base(entityId, $"an entity with the value '{conflictingValue}' for property '{propertyName}' already exists")
        {
        }

        private UniquenessConstraintViolationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Details { get; init; } = string.Empty;

        public override string Type => "https://cac.com/errors/uniqueness-constraint-violation";
    }
}
