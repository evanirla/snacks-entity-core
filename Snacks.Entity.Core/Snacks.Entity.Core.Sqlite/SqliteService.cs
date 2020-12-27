using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Snacks.Entity.Core.Database;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Sqlite
{
    public class SqliteService : BaseDbService<SqliteConnection>
    {
        public SqliteService(IOptions<SqliteOptions> options) : base(options.Value.ConnectionString)
        {

        }

        public override async Task<SqliteConnection> GetConnectionAsync()
        {
            SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public override Task<int> GetLastInsertId(IDbTransaction transaction)
        {
            return QuerySingleAsync<int>("select LAST_INSERT_ROWID()", null, transaction);
        }
    }
}
