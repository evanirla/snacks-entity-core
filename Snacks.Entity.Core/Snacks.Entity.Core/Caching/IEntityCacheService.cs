using Microsoft.AspNetCore.Http;
using Snacks.Entity.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Caching
{
    public interface IEntityCacheService<TModel>
        where TModel : IEntityModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<TModel> GetCustomOneAsync(string cacheKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<IList<TModel>> GetCustomManyAsync(string cacheKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TModel> GetOneAsync(dynamic key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        Task<IList<TModel>> GetManyAsync(IQueryCollection queryCollection);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SetCustomOneAsync(string cacheKey, TModel model);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="queryCollection"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        Task SetCustomManyAsync(string cacheKey, IList<TModel> models);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SetOneAsync(TModel model);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        Task SetManyAsync(IQueryCollection queryCollection, IList<TModel> models);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task RemoveOneAsync(TModel model);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveOneAsync(dynamic key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        Task RemoveManyAsync(IQueryCollection queryCollection = null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntityCacheService<TModel, TKey> : IEntityCacheService<TModel>
        where TModel : IEntityModel<TKey>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TModel> GetOneAsync(TKey key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveOneAsync(TKey key);
    }
}
