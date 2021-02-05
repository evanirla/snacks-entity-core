using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Snacks.Entity.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
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
