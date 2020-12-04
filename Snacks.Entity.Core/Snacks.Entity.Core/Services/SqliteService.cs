using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class SqliteService : IDbService<SqliteConnection>
    {
        bool _initialized;
        readonly string _connectionString;

        public SqliteService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExecuteSqlAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
                await connection.ExecuteAsync(sql, parameters);
            }
            else
            {
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }
        }

        public async Task ExecuteSqlAsync(string sql, DynamicParameters parameters, IDbTransaction transaction = null)
        {
            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
                await connection.ExecuteAsync(sql, parameters, transaction);
            }
            else
            {
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }
        }

        public async Task<SqliteConnection> GetConnectionAsync()
        {
            SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            using SqliteConnection connection = await GetConnectionAsync();
            await connection.OpenAsync();

            _initialized = true;
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<dynamic> rows;

            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
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

            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync<T>(sql, parameters, transaction); ;
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

            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
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

            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
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
            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
                return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
            }
            else
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
            }
        }

        public async Task<T> QuerySingleAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            if (transaction == null)
            {
                using SqliteConnection connection = await GetConnectionAsync();
                return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
            }
            else
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
            }
        }
    }
}
