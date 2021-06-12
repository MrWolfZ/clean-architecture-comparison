using System;
using System.Globalization;

namespace CAC.Core.Domain
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToIsoString(this DateTimeOffset dateTimeOffset) => dateTimeOffset.ToString("o", CultureInfo.InvariantCulture);
        
        public static DateTimeOffset FromIsoString(this string isoString) => DateTimeOffset.ParseExact(isoString, "o", CultureInfo.InvariantCulture);
    }
}
