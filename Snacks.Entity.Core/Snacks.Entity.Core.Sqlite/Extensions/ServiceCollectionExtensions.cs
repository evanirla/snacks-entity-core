using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Database;

namespace Snacks.Entity.Core.Sqlite.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteService(this IServiceCollection services, string connectionString)
        {
            return services.AddSingleton<IDbService<SqliteConnection>>(new SqliteService(connectionString));
        }
    }
}
