using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Snacks.Entity.Core.Database;

namespace Snacks.Entity.Core.MySql.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMySqlService(this IServiceCollection services, string connectionString)
        {
            return services.AddSingleton<IDbService<MySqlConnection>>(new MySqlService(connectionString));
        }
    }
}
