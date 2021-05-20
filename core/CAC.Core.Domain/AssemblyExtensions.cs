using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CAC.Core.Domain
{
    public static class AssemblyExtensions
    {
        public static void AddTypeConverterAttributes(this Assembly assembly)
        {
            foreach (var type in FindEntityIdTypes(assembly))
            {
                AddTypeConverterAttribute(type);
            }
        }
        
        private static void AddTypeConverterAttribute(Type type)
        {
            var genericConverterType = typeof(EntityIdTypeConverter<>);
            var concreteConverterType = genericConverterType.MakeGenericType(type);
            TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(concreteConverterType));
        }
        
        private static IEnumerable<Type> FindEntityIdTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(EntityId)));
        }
    }
}
