using System;

namespace UGameCore.Utilities
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }

        /// <summary>
        /// Gets service of given type, but throws exception if it was not found.
        /// </summary>
        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            object service = provider.GetService(serviceType);
            if (service == null)
                throw new InvalidOperationException($"Required service of type {serviceType.Name} not found in service provider {provider}");
            return service;
        }

        /// <summary>
        /// Gets service of given type, but throws exception if it was not found.
        /// </summary>
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetRequiredService(typeof(T));
        }
    }
}
