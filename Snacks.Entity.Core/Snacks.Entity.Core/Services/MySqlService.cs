using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class MySqlService : IDbService<MySqlConnection>
    {
        readonly string _connectionString;
        readonly ILogger<MySqlService> _logger;

        bool _initialized;

        public MySqlService(string connectionString, ILogger<MySqlService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing Database service.");

            using (var connection = await GetConnectionAsync())
            {
                await connection.QueryAsync("select * from information_schema.tables;");
            }

            _logger.LogInformation("Database service initialization complete.");

            _initialized = true;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<dynamic> rows;

            _logger.LogDebug("QueryAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync(sql, parameters, transaction);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync(sql, parameters, transaction);
            }

            return rows;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<T> rows;

            _logger.LogDebug("QueryAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync<T>(sql, parameters);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync<T>(sql, parameters, transaction);
            }

            return rows;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<dynamic> rows;

            _logger.LogDebug("QueryAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync(sql, parameters, transaction);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync(sql, parameters, transaction);
            }

            return rows;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<T> rows;

            _logger.LogDebug("QueryAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync<T>(sql, parameters, transaction);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync<T>(sql, parameters, transaction);
            }

            return rows;
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            _logger.LogDebug("QuerySingleAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                T val = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
                return val;
            }
            else
            {
                return await transaction.Connection.QuerySingleAsync<T>(sql, parameters, transaction);
            }
        }

        public async Task<T> QuerySingleAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            _logger.LogDebug("QuerySingleAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                transaction ??= await connection.BeginTransactionAsync();

                T val = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);

                transaction.Commit();

                return val;
            }
            else
            {
                return await transaction.Connection.QuerySingleAsync<T>(sql, parameters, transaction);
            }
        }

        public async Task ExecuteSqlAsync(string sql, DynamicParameters parameters, IDbTransaction transaction = null)
        {
            _logger.LogDebug("ExecuteSqlAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                transaction ??= await connection.BeginTransactionAsync();

                await connection.ExecuteAsync(sql, parameters, transaction);

                transaction.Commit();
            }
            else
            {
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }
        }

        public async Task ExecuteSqlAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            _logger.LogDebug("ExecuteSqlAsync:");
            _logger.LogDebug("  sql => {0}", sql);
            _logger.LogDebug("  parameters => {0}", JsonSerializer.Serialize(parameters));

            if (transaction == null)
            {
                using MySqlConnection connection = await GetConnectionAsync();
                transaction ??= await connection.BeginTransactionAsync();

                await connection.ExecuteAsync(sql, parameters, transaction);

                transaction.Commit();
            }
            else
            {
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            _logger.LogInformation("Opening MySql connection...");
            await connection.OpenAsync();
            _logger.LogInformation("MySql connection successful");
            return connection;
        }
    }
}
