using System.Collections.Immutable;

namespace CAC.Core.Domain
{
    public static class ValueListExtensions
    {
        public static ValueList<T> EmptyIfNull<T>(this ValueList<T>? list)
            where T : notnull =>
            list ?? ValueList<T>.Empty;
    }
}
