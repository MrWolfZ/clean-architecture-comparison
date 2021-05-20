using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CAC.Core.Domain
{
    internal sealed class EntityIdJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(EntityId));

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(EntityIdJsonConverter<>).MakeGenericType(typeToConvert);
            return Activator.CreateInstance(converterType) as JsonConverter;
        }
    }
}
