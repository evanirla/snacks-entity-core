using Dapper;
using Microsoft.Extensions.Logging;
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
        public MySqlService(string connectionString) : base(connectionString)
        {

        }

        public override async Task<MySqlConnection> GetConnectionAsync()
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
