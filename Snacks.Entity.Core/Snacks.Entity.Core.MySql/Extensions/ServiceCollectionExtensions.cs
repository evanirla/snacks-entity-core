using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Snacks.Entity.Core.Database;
using System;

namespace Snacks.Entity.Core.MySql.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMySqlService(this IServiceCollection services, Action<MySqlOptions> setupAction)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services.AddSingleton<IDbService<MySqlConnection>, MySqlService>();
        }
    }
}
