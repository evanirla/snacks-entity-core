using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Caching
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class EntityCacheService<TModel, TKey> : IEntityCacheService<TModel, TKey>
        where TModel : IEntityModel<TKey>
    {
        protected readonly IDistributedCache _distributedCache;
        protected readonly PropertyInfo _primaryKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distributedCache"></param>
        public EntityCacheService(
            IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _primaryKey = typeof(TModel).GetProperties()
                .FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task RemoveOneAsync(TModel model)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(model));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task RemoveOneAsync(TKey key)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        public async Task<IList<TModel>> GetManyAsync(IQueryCollection queryCollection)
        {
            byte[] modelListData =
                await _distributedCache.GetAsync(GetModelListKey(queryCollection));

            if (modelListData != null)
            {
                return modelListData.ToObject<List<TModel>>();
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TModel> GetOneAsync(TKey key)
        {
            byte[] modelData =
                await _distributedCache.GetAsync(GetCacheKey(key));

            if (modelData != null)
            {
                return modelData.ToObject<TModel>();
            }

            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task SetManyAsync(IQueryCollection queryCollection, IList<TModel> models)
        {
            await _distributedCache.SetAsync(GetModelListKey(queryCollection), models.ToByteArray());
            await AddModelListKey(queryCollection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetOneAsync(TModel model)
        {
            await _distributedCache.SetAsync(GetCacheKey(model), model.ToByteArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<TModel> GetCustomOneAsync(string cacheKey)
        {
            byte[] modelData = await _distributedCache.GetAsync(cacheKey);

            if (modelData != null)
            {
                return modelData.ToObject<TModel>();
            }

            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        public async Task<IList<TModel>> GetCustomManyAsync(string cacheKey, IQueryCollection queryCollection)
        {
            byte[] modelListData = await _distributedCache.GetAsync(
                cacheKey + $"({queryCollection.Select(x => $"{x.Key}={x.Value}")})");

            if (modelListData != null)
            {
                return modelListData.ToObject<List<TModel>>();
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetCustomOneAsync(string cacheKey, TModel model)
        {
            await _distributedCache.SetAsync(cacheKey, model.ToByteArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="queryCollection"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task SetCustomManyAsync(string key, IQueryCollection queryCollection, IList<TModel> models)
        {
            await _distributedCache.SetAsync(
                key + $"({queryCollection.Select(x => $"{x.Key}={x.Value}")})", models.ToByteArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        public async Task RemoveManyAsync(IQueryCollection queryCollection = null)
        {
            if (queryCollection != null)
            {
                await _distributedCache.RemoveAsync(GetModelListKey(queryCollection));
            }
            else
            {
                foreach (string key in await GetModelListKeysAsync())
                {
                    await _distributedCache.RemoveAsync(key);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetCacheKey(TModel model)
        {
            TKey key = (TKey)_primaryKey.GetValue(model);
            return $"{typeof(TModel).Name}({key})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetCacheKey(TKey key)
        {
            return $"{typeof(TModel).Name}({key})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        private string GetModelListKey(IQueryCollection queryCollection)
        {
            string queryCollectionString = string.Empty;

            if (queryCollection != null)
            {
                queryCollectionString =
                    string.Join(",", queryCollection.Select(x => $"{x.Key}={x.Value}"));
            }
            else
            {
                queryCollectionString = "None";
            }

            return $"{typeof(TModel).Name}({queryCollectionString})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetModelListKeysAsync()
        {
            string key = $"{typeof(TModel).Name}Keys";

            byte[] keyListData = await _distributedCache.GetAsync(key);

            if (keyListData != null)
            {
                return keyListData.ToObject<List<string>>();
            }

            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryCollection"></param>
        /// <returns></returns>
        private async Task AddModelListKey(IQueryCollection queryCollection)
        {
            string key = $"{typeof(TModel).Name}Keys";

            List<string> keys = await GetModelListKeysAsync();

            if (keys == null)
            {
                keys = new List<string>();
            }

            keys.Add(GetModelListKey(queryCollection));

            await _distributedCache.SetAsync(key, keys.ToByteArray());
        }
    }
}
