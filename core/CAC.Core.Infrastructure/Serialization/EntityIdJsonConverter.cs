using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CAC.Core.Domain;

namespace CAC.Core.Infrastructure.Serialization
{
    public sealed class EntityIdJsonConverter<T> : JsonConverter<T>
        where T : EntityId
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert != typeof(T))
            {
                throw new ArgumentException($"expected type {typeof(T).Name} but got {typeToConvert.Name}", nameof(typeToConvert));
            }

            return EntityId.Parse<T>(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
