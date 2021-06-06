using System;
using System.Linq;
using System.Reflection;

namespace CAC.Core.Domain
{
    public abstract record EntityId<T> : EntityId
    {
        protected EntityId(long numericId)
            : base(numericId, typeof(T))
        {
        }
    }

    public abstract record EntityId
    {
        protected EntityId(long numericId, Type entityType)
        {
            if (numericId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numericId), "entity id numeric value must be a positive integer");
            }

            if (!entityType.IsAssignableTo(typeof(Entity)) && !entityType.IsAssignableTo(typeof(AggregateRoot)))
            {
                throw new ArgumentException($"type {entityType.Name} must be a subtype of {nameof(Entity)} or {nameof(AggregateRoot)}", nameof(entityType));
            }

            NumericValue = numericId;
            Value = $"{entityType.Name}-{numericId}";
        }

        public long NumericValue { get; }

        public string Value { get; }

        public override string ToString() => Value;

        internal static T? Parse<T>(string? s)
            where T : EntityId
        {
            var entityIdType = typeof(T);
            var baseType = entityIdType.BaseType;

            if (baseType == null || baseType.GetGenericTypeDefinition() != typeof(EntityId<>))
            {
                throw new ArgumentException($"type {entityIdType.Name} must be a sub-type of the generic variant of {nameof(EntityId)}");
            }

            if (!TryExtractNumericId(out var numericId))
            {
                throw new ArgumentException($"{s} is not a valid entity id value for type {entityIdType.Name}", nameof(s));
            }

            return CreateInstance();

            bool TryExtractNumericId(out long result)
            {
                var entityType = baseType.GenericTypeArguments.Single();
                var typePrefix = $"{entityType.Name}-";
                if (s == null || !s.StartsWith(typePrefix))
                {
                    result = 0;
                    return false;
                }

                var numericPart = s[typePrefix.Length..];

                return long.TryParse(numericPart, out result);
            }

            T? CreateInstance()
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var parameterTypes = new[] { typeof(long) };
                var constructorInfo = entityIdType.GetConstructor(flags, null, parameterTypes, null);
                return (T?)constructorInfo?.Invoke(new object?[] { numericId });
            }
        }

        internal static bool IsEntityIdType(Type type)
        {
            return type.BaseType is { IsGenericType: true } && type.BaseType.GetGenericTypeDefinition() == typeof(EntityId<>);
        }

        internal static string CreateExampleValue(Type entityIdType)
        {
            if (!IsEntityIdType(entityIdType))
            {
                throw new ArgumentException($"type {entityIdType.Name} must be a sub-type of the generic variant of {nameof(EntityId)}", nameof(entityIdType));
            }

            var entityType = entityIdType.BaseType?.GenericTypeArguments.Single();
            return $"{entityType?.Name}-1";
        }
    }
}
