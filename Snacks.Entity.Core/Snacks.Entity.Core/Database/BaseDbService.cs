using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    public abstract class BaseDbService<TDbConnection> : IDbService<TDbConnection>
        where TDbConnection : IDbConnection
    {
        protected readonly string _connectionString;

        public BaseDbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual async Task ExecuteSqlAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            if (transaction == null)
            {
                using TDbConnection connection = await GetConnectionAsync();
                await connection.ExecuteAsync(sql, parameters);
            }
            else
            {
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }
        }

        public virtual async Task ExecuteSqlAsync(string sql, DynamicParameters parameters, IDbTransaction transaction = null)
        {
            await ExecuteSqlAsync(sql, (object)parameters, transaction);
        }

        public virtual async Task InitializeAsync()
        {
            using var connection = await GetConnectionAsync();
        }

        public virtual async Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<dynamic> rows;

            if (transaction == null)
            {
                using TDbConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync(sql, parameters);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync(sql, parameters, transaction);
            }

            return rows;
        }

        public virtual async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            IEnumerable<T> rows;

            if (transaction == null)
            {
                using TDbConnection connection = await GetConnectionAsync();
                return await connection.QueryAsync<T>(sql, parameters);
            }
            else
            {
                rows = await transaction.Connection.QueryAsync<T>(sql, parameters, transaction);
            }

            return rows;
        }

        public virtual async Task<IEnumerable<dynamic>> QueryAsync(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            return await QueryAsync(sql, (object)parameters, transaction);
        }

        public virtual async Task<IEnumerable<T>> QueryAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            return await QueryAsync<T>(sql, (object)parameters, transaction);
        }

        public virtual async Task<T> QuerySingleAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            if (transaction == null)
            {
                using TDbConnection connection = await GetConnectionAsync();
                T val = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
                return val;
            }
            else
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
            }
        }

        public virtual async Task<T> QuerySingleAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null)
        {
            return await QuerySingleAsync<T>(sql, (object)parameters, transaction);
        }

        public abstract Task<TDbConnection> GetConnectionAsync();

        async Task<IDbConnection> IDbService.GetConnectionAsync()
        {
            return await GetConnectionAsync();
        }
    }
}
