using Dapper;
using Microsoft.Data.Sqlite;
using Snacks.Entity.Core.Database;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Sqlite
{
    public class SqliteService : BaseDbService<SqliteConnection>
    {
        public SqliteService(string connectionString) : base(connectionString)
        {
        }

        public override async Task<SqliteConnection> GetConnectionAsync()
        {
            SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
