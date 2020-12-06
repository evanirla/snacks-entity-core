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
    public class EntityCacheService<TModel, TKey> : IEntityCacheService<TModel, TKey>
        where TModel : IEntityModel<TKey>
    {
        protected readonly IDistributedCache _distributedCache;
        protected readonly PropertyInfo _primaryKey;

        public EntityCacheService(
            IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _primaryKey = typeof(TModel).GetProperties(BindingFlags.Public)
                .FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));
        }

        public async Task RemoveOneAsync(TModel model)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(model));
        }

        public async Task RemoveOneAsync(TKey key)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(key));
        }

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

        public async Task SetManyAsync(IQueryCollection queryCollection, IList<TModel> models)
        {
            await _distributedCache.SetAsync(GetModelListKey(queryCollection), models.ToByteArray());
            await AddModelListKey(queryCollection);
        }

        public async Task SetOneAsync(TModel model)
        {
            await _distributedCache.SetAsync(GetCacheKey(model), model.ToByteArray());
        }

        public async Task<TModel> GetCustomOneAsync(string cacheKey)
        {
            byte[] modelData = await _distributedCache.GetAsync(cacheKey);

            if (modelData != null)
            {
                return modelData.ToObject<TModel>();
            }

            return default;
        }

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

        public async Task SetCustomOneAsync(string cacheKey, TModel model)
        {
            await _distributedCache.SetAsync(cacheKey, model.ToByteArray());
        }

        public async Task SetCustomManyAsync(string key, IQueryCollection queryCollection, IList<TModel> models)
        {
            await _distributedCache.SetAsync(
                key + $"({queryCollection.Select(x => $"{x.Key}={x.Value}")})", models.ToByteArray());
        }

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

        private string GetCacheKey(TModel model)
        {
            TKey key = (TKey)_primaryKey.GetValue(model);
            return $"{typeof(TModel).Name}({key})";
        }

        private string GetCacheKey(TKey key)
        {
            return $"{typeof(TModel).Name}({key})";
        }

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
