using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Database;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Entity
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TDbService"></typeparam>
    public interface IEntityService<TModel, TKey, TDbService, TDbConnection> 
        where TModel : IEntityModel<TKey> 
        where TDbConnection : IDbConnection
        where TDbService : IDbService<TDbConnection>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TModel> GetOneAsync(TKey key, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        Task<IList<TModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        Task<IList<TModel>> GetManyAsync(Dictionary<string, StringValues> queryCollection, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<TModel> CreateOneAsync(TModel model, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IList<TModel>> CreateManyAsync(IList<TModel> models, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task UpdateOneAsync(TModel model, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task DeleteOneAsync(TKey key, IDbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task DeleteOneAsync(TModel model, IDbTransaction transaction = null);

        Task InitializeAsync();
    }
}
