using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbService<TDbConnection> where TDbConnection : IDbConnection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryAsync(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<T> QuerySingleAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<T> QuerySingleAsync<T>(string sql, DynamicParameters parameters = null, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task ExecuteSqlAsync(string sql, object parameters, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task ExecuteSqlAsync(string sql, DynamicParameters parameters, IDbTransaction transaction = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<TDbConnection> GetConnectionAsync();
    }
}
