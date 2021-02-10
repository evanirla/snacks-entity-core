using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snacks.Entity.Caching
{
    public abstract class EntityCacheServiceBase<TEntity> : IEntityCacheService<TEntity>
        where TEntity : class
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string _listKey = $"{typeof(TEntity).Name}Keys";

        public EntityCacheServiceBase(
            IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task AddAsync(HttpRequest httpRequest, TEntity model, DistributedCacheEntryOptions options = null)
        {
            string cacheKey = GetCacheKey(httpRequest);
            await WriteToCacheAsync(cacheKey, model, options).ConfigureAwait(false);
            await AddCacheKeyAsync(cacheKey).ConfigureAwait(false);
        }

        public async Task AddAsync(HttpRequest httpRequest, IList<TEntity> models, DistributedCacheEntryOptions options = null)
        {
            string cacheKey = GetCacheKey(httpRequest);
            await WriteToCacheAsync(cacheKey, models, options).ConfigureAwait(false);
            await AddCacheKeyAsync(cacheKey).ConfigureAwait(false);
        }

        public async Task AddRelatedAsync<TRelated>(HttpRequest request, IList<TRelated> relatedModels, DistributedCacheEntryOptions options = null) where TRelated : class
        {
            string cacheKey = GetCacheKey(request);
            await WriteToCacheAsync(cacheKey, relatedModels, options).ConfigureAwait(false);
            await AddCacheKeyAsync(cacheKey).ConfigureAwait(false);
        }

        public virtual async Task AddValueAsync<TValue>(HttpRequest httpRequest, TEntity model, TValue value, DistributedCacheEntryOptions options = null)
        {
            string cacheKey = GetCacheKey(httpRequest);
            await WriteToCacheAsync(cacheKey, value, options).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> FindAsync(HttpRequest httpRequest)
        {
            string cacheKey = GetCacheKey(httpRequest);
            return await ReadFromCacheAsync<TEntity>(cacheKey).ConfigureAwait(false);
        }

        public virtual async Task<IList<TEntity>> GetAsync(HttpRequest httpRequest)
        {
            string cacheKey = GetCacheKey(httpRequest);
            return await ReadFromCacheAsync<List<TEntity>>(cacheKey).ConfigureAwait(false);
        }

        public async Task PurgeAsync()
        {
            List<string> cacheKeys = await GetCacheKeysAsync().ConfigureAwait(false);
            foreach (string cacheKey in cacheKeys)
            {
                await _distributedCache.RemoveAsync(cacheKey).ConfigureAwait(false);
            }
            await _distributedCache.RemoveAsync(_listKey).ConfigureAwait(false);
        }

        public virtual async Task<TValue> GetValueAsync<TValue>(HttpRequest request, TEntity model)
        {
            string cacheKey = GetCacheKey(request);
            TValue value = await ReadFromCacheAsync<TValue>(cacheKey).ConfigureAwait(false);
            return value;
        }

        public async Task<IList<TRelated>> GetRelatedAsync<TRelated>(HttpRequest request) where TRelated : class
        {
            string cacheKey = GetCacheKey(request);
            return await ReadFromCacheAsync<IList<TRelated>>(cacheKey).ConfigureAwait(false);
        }

        protected virtual string GetCacheKey(HttpRequest request)
        {
            StringBuilder sb = new StringBuilder(typeof(TEntity).Name);
            sb.Append("(");
            sb.Append(request.Path);
            sb.AppendFormat("&Method={0}", request.Method);

            foreach (var param in request.Query)
            {
                sb.AppendFormat("&{0}={1}", param.Key, param.Value);
            }

            sb.Append(")");

            return sb.ToString();
        }

        private async Task AddCacheKeyAsync(string cacheKey)
        {
            List<string> cacheKeys = await ReadFromCacheAsync<List<string>>(_listKey).ConfigureAwait(false);

            if (cacheKeys == null)
            {
                cacheKeys = new List<string>();
            }

            if (cacheKeys.Contains(cacheKey))
            {
                cacheKeys.Add(cacheKey);
                await WriteToCacheAsync(_listKey, cacheKeys).ConfigureAwait(false);
            }
        }

        private async Task<List<string>> GetCacheKeysAsync()
        {
            return await ReadFromCacheAsync<List<string>>(_listKey);
        }

        private async Task WriteToCacheAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options = null)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, data);
            stream.Position = 0;
            byte[] byteData = new byte[stream.Length];
            await stream.ReadAsync(byteData);

            if (options != null)
            {
                await _distributedCache.SetAsync(cacheKey, byteData, options);
            }
            else
            {
                await _distributedCache.SetAsync(cacheKey, byteData);
            }
        }

        protected async Task<T> ReadFromCacheAsync<T>(string cacheKey)
        {
            byte[] data = await _distributedCache.GetAsync(cacheKey);

            if (data != null && data.Length > 0)
            {
                using var stream = new MemoryStream();
                await stream.WriteAsync(data);
                stream.Position = 0;
                return await JsonSerializer.DeserializeAsync<T>(stream);
            }

            return default;
        }
    }
}
