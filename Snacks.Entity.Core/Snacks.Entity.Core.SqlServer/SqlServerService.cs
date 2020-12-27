using Microsoft.Extensions.Options;
using Snacks.Entity.Core.Database;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.SqlServer
{
    public class SqlServerService : BaseDbService<SqlConnection>
    {
        readonly SqlServerOptions _options;

        public SqlServerService(IOptions<SqlServerOptions> options) : base(options.Value.ConnectionString)
        {
            _options = options.Value;
        }

        public override async Task<SqlConnection> GetConnectionAsync()
        {
            SqlConnection connection = _options.Credential != null ? 
                new SqlConnection(_connectionString) : new SqlConnection(_connectionString, _options.Credential);
            await connection.OpenAsync();
            return connection;
        }

        public override async Task<int> GetLastInsertId(IDbTransaction transaction)
        {
            return await QuerySingleAsync<int>("select SCOPE_IDENTITY()", null, transaction);
        }
    }
}
