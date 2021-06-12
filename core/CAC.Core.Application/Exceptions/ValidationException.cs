using System;
using System.Runtime.Serialization;
using CAC.Core.Domain;

// property Type should have that name to match the naming of ProblemDetails
#pragma warning disable CA1721

// we don't want to allow other constructors than the one with a single message (plus the init props)
#pragma warning disable CA1032

namespace CAC.Core.Application.Exceptions
{
    [Serializable]
    public sealed class ValidationException : Exception
    {
        public ValidationException(string message, EntityId? entityId = null)
            : base(message)
        {
            EntityId = entityId;
        }

        // ignore non-nullable field uninitialized for serialization constructor
#pragma warning disable CS8618
        private ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore CS8618

        public string Details { get; init; } = string.Empty;

        public string Type => "https://cac.com/errors/validation-violation";

        public EntityId? EntityId { get; }
    }
}
