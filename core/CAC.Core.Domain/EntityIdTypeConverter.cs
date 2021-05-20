using System;
using System.ComponentModel;
using System.Globalization;

namespace CAC.Core.Domain
{
    public sealed class EntityIdTypeConverter<TEntityId> : TypeConverter
        where TEntityId : EntityId
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return EntityId.Parse<TEntityId>(value as string);
        }
    }
}
