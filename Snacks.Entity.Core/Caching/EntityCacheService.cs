using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Caching
{
    /// <summary>
    /// Utilized by Entity Services to cache query results
    /// </summary>
    /// <typeparam name="TModel">The entity type to cache</typeparam>
    public class EntityCacheService<TModel> : IEntityCacheService<TModel>
        where TModel : IEntityModel
    {
        protected readonly IDistributedCache _distributedCache;
        protected readonly PropertyInfo _primaryKey;
        protected readonly EntityCacheOptions _options;

        public EntityCacheService(
            IDistributedCache distributedCache,
            IOptions<EntityCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _options = options.Value;
            _primaryKey = typeof(TModel).GetProperties()
                .FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));
        }

        public async Task RemoveOneAsync(TModel model)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(model.Key));
        }

        public async Task RemoveOneAsync(object key)
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

            return default;
        }

        public async Task<TModel> GetOneAsync(object key)
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
            await _distributedCache.SetAsync(GetModelListKey(queryCollection), models.ToByteArray(), GetEntryOptions());
            await AddModelListKey(queryCollection);
        }

        public async Task SetOneAsync(TModel model)
        {
            string cacheKey = GetCacheKey(model.Key);
            await _distributedCache.SetAsync(cacheKey, model.ToByteArray(), GetEntryOptions());
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

        public async Task<IList<TModel>> GetCustomManyAsync(string cacheKey)
        {
            byte[] modelListData = await _distributedCache.GetAsync(cacheKey);

            if (modelListData != null)
            {
                return modelListData.ToObject<List<TModel>>();
            }

            return default;
        }

        public async Task SetCustomOneAsync(string cacheKey, TModel model)
        {
            await _distributedCache.SetAsync(cacheKey, model.ToByteArray(), GetEntryOptions());
        }

        public async Task SetCustomManyAsync(string cacheKey, IList<TModel> models)
        {
            await _distributedCache.SetAsync(cacheKey, models.ToByteArray(), GetEntryOptions());
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

        protected string GetCacheKey(object key)
        {
            return $"{typeof(TModel).Name}({key})";
        }

        protected string GetModelListKey(IQueryCollection queryCollection)
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

        protected async Task<List<string>> GetModelListKeysAsync()
        {
            string key = $"{typeof(TModel).Name}Keys";

            byte[] keyListData = await _distributedCache.GetAsync(key);

            if (keyListData != null)
            {
                return keyListData.ToObject<List<string>>();
            }

            return new List<string>();
        }

        protected async Task AddModelListKey(IQueryCollection queryCollection)
        {
            string key = $"{typeof(TModel).Name}Keys";

            List<string> keys = await GetModelListKeysAsync();

            if (keys == null)
            {
                keys = new List<string>();
            }

            keys.Add(GetModelListKey(queryCollection));

            await _distributedCache.SetAsync(key, keys.ToByteArray(), GetEntryOptions());
        }

        protected DistributedCacheEntryOptions GetEntryOptions()
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();

            if (_options.EntryAction != null)
            {
                _options.EntryAction.Invoke(entryOptions);
            }

            return entryOptions;
        }
    }

    public class EntityCacheService<TModel, TKey> : EntityCacheService<TModel>
        where TModel : IEntityModel<TKey>
    {
        public EntityCacheService(
            IDistributedCache distributedCache,
            IOptions<EntityCacheOptions> options) : base(distributedCache, options)
        {
        }

        public async Task RemoveOneAsync(TKey key)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(key));
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
    }
}
