﻿using Microsoft.Extensions.DependencyInjection;
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
                Type modelType = GetModelTypeFromServiceType(serviceType);

                if (modelType != null)
                {
                    services.AddSingleton(typeof(IEntityService<>).MakeGenericType(modelType), serviceType);
                }
            }

            return services;
        }

        private static Type GetModelTypeFromServiceType(Type serviceType)
        {
            Type modelType = serviceType.BaseType.GetGenericArguments().FirstOrDefault();

            if (modelType == null)
            {
                modelType = serviceType.GetInterface("IEntityService`1").GetGenericArguments().FirstOrDefault();
            }

            return modelType;
        }
    }
}
