using System;
using System.Runtime.Serialization;

// property Type should have that name to match the naming of ProblemDetails
#pragma warning disable CA1721

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Domain.Exceptions
{
    [Serializable]
    public class DomainInvariantViolationException : DomainEntityException
    {
        public DomainInvariantViolationException(EntityId entityId, string message)
            : base(entityId, message)
        {
        }

        // ignore non-nullable field uninitialized for serialization constructor
#pragma warning disable CS8618
        protected DomainInvariantViolationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore CS8618

        public override string Details { get; init; } = string.Empty;

        public override string Type => "https://cac.com/errors/domain-invariant-violation";
    }
}
