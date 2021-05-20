using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CAC.Core.Domain
{
    internal sealed class ValueListJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ValueList<>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(ValueListJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments().Single());
            return Activator.CreateInstance(converterType) as JsonConverter;
        }
    }
}
