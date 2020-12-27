using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Database;
using System;

namespace Snacks.Entity.Core.Sqlite.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteService(this IServiceCollection services, Action<SqliteOptions> setupAction)
        {
            if (setupAction != null)
            {
                services.Configure(setupAction);    
            }

            return services.AddSingleton<IDbService<SqliteConnection>, SqliteService>();
        }
    }
}
