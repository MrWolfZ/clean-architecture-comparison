using System;
using Microsoft.Extensions.DependencyInjection;

namespace CAC.Core.Application
{
    internal static class ServiceProviderExtensions
    {
        public static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType != null)
            {
                return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(provider);
            }

            throw new InvalidOperationException("cannot instantiate descriptor");
        }
        
        public static object CreateInstance(this IServiceProvider provider, Type type, params object[] extraArguments)
        {
            return ActivatorUtilities.CreateInstance(provider, type, extraArguments);
        }
        
        private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }
    }
}
