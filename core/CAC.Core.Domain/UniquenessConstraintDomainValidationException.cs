using System;
using System.Runtime.Serialization;

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Domain
{
    [Serializable]
    public sealed class UniquenessConstraintDomainValidationException : DomainValidationException
    {
        public UniquenessConstraintDomainValidationException(EntityId entityId, string propertyName, object conflictingValue)
            : base(entityId, $"an entity with the value '{conflictingValue}' for property '{propertyName}' already exists")
        {
        }

        private UniquenessConstraintDomainValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Type => "https://cac.com/errors/domain-validation/uniqueness-constraint";
    }
}
