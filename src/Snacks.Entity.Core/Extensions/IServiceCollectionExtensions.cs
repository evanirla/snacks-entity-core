using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Snacks.Entity.Core.Extensions
{
    /// <summary>
    /// Extensions to simplify registering services
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all implementations of <see cref="IEntityService{TEntity}"/> to the service collection.
        /// </summary>
        /// <remarks>
        /// Intended to be called within the <c>ConfigureServices</c> method in <c>Startup.cs</c>
        /// </remarks>
        /// <example>
        /// <code>
        /// services.AddEntityServices();
        /// </code>
        /// </example>
        /// <param name="services">The collection of services</param>
        /// <returns>The service collection so calls can be chained</returns>
        public static IServiceCollection AddEntityServices(this IServiceCollection services)
        {
            Type[] serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => x.GetInterface("IEntityService`1") != null)
                .ToArray();

            foreach (Type serviceType in serviceTypes)
            {
                Type modelType = serviceType.GetInterface("IEntityService`1").GetGenericArguments().SingleOrDefault();
                services.AddSingleton(typeof(IEntityService<>).MakeGenericType(modelType), serviceType);
            }

            return services;
        }
    }
}
