using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Snacks.Entity.Core.Database;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.MySql
{
    public class MySqlService : BaseDbService<MySqlConnection>
    {
        public MySqlService(IOptions<MySqlOptions> options) : base(options.Value.ConnectionString)
        {

        }

        public override async Task<MySqlConnection> GetConnectionAsync()
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public override Task<int> GetLastInsertId(IDbTransaction transaction)
        {
            return QuerySingleAsync<int>(
                "select last_insert_id();", null, transaction);
        }
    }
}
