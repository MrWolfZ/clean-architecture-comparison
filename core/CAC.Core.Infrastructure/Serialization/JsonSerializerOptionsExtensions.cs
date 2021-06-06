using System.Text.Json;

namespace CAC.Core.Infrastructure.Serialization
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddCoreConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new EntityIdJsonConverterFactory());
            options.Converters.Add(new ValueListJsonConverterFactory());

            return options;
        } 
    }
}
