using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Snacks.Entity.Core.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Snacks.Entity.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/> to simplify service registration
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a new application model provider for creating <see cref="ControllerBase"/>
        /// instances for a <see cref="DbContext" />
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEntityProvider<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            return services.AddTransient<IApplicationModelProvider, EntityApplicationModelProvider<TDbContext>>();
        }
    }
}
