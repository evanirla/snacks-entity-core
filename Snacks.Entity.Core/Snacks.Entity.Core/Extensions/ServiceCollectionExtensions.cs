using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Caching;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snacks.Entity.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        static IEnumerable<Type> EntityServiceTypes =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.FullName.Contains("Snacks.Entity.Core"))
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IEntityService).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract && !x.IsInterface);

        public static IServiceCollection AddEntityServices(this IServiceCollection services)
        {
            foreach (Type serviceType in EntityServiceTypes)
            {
                Type modelType = serviceType.BaseType.GetGenericArguments().First();
                services.AddSingleton(typeof(IEntityService<>).MakeGenericType(modelType), serviceType);
            }

            return services;
        }

        public static IServiceCollection AddEntityCacheServices(this IServiceCollection services)
        {
            foreach (Type serviceType in EntityServiceTypes)
            {
                Type modelType = serviceType.BaseType.GetGenericArguments().First();
                services.AddSingleton(typeof(IEntityCacheService<>).MakeGenericType(modelType),
                    typeof(EntityCacheService<>).MakeGenericType(modelType));
            }

            return services;
        }
    }
}
