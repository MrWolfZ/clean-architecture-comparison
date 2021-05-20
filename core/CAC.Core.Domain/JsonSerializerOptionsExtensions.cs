using System.Text.Json;

namespace CAC.Core.Domain
{
    public static class JsonSerializerOptionsExtensions
    {
        public static void AddCoreConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new EntityIdJsonConverterFactory());
            options.Converters.Add(new ValueListJsonConverterFactory());
        } 
    }
}
