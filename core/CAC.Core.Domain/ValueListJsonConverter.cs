using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CAC.Core.Domain
{
    internal sealed class ValueListJsonConverter<T> : JsonConverter<ValueList<T>>
        where T : notnull
    {
        public override ValueList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var arrayConverter = GetArrayConverter(options);
            var result = arrayConverter.Read(ref reader, typeof(T).MakeArrayType(), options);
            return result?.ToValueList();
        }

        public override void Write(Utf8JsonWriter writer, ValueList<T> value, JsonSerializerOptions options)
        {
            var arrayConverter = GetArrayConverter(options);
            arrayConverter.Write(writer, value.ToArray(), options);
        }

        private static JsonConverter<T[]> GetArrayConverter(JsonSerializerOptions options) => (JsonConverter<T[]>)options.GetConverter(typeof(T).MakeArrayType());
    }
}
