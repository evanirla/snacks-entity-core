using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class DistributedCacheHelper<TController> : IDistributedCacheHelper<TController>
        where TController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DistributedCacheHelper(
            IDistributedCache distributedCache,
            JsonSerializerOptions jsonSerializerOptions = null)
        {
            _distributedCache = distributedCache;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        /// <inheritdoc/>
        public async Task AddRequestAsync<TValue>(HttpRequest httpRequest, TValue value, DistributedCacheEntryOptions options = null)
        {
            if (_distributedCache == null)
            {
                return;
            }

            string cacheKey = GetCacheKey(httpRequest);
            await WriteToCacheAsync(cacheKey, value, options);
            await AddCacheKeyAsync(cacheKey);
        }

        /// <inheritdoc/>
        public async Task<TValue> GetFromRequestAsync<TValue>(HttpRequest httpRequest)
        {
            if (_distributedCache == null)
            {
                return default;
            }

            string cacheKey = GetCacheKey(httpRequest);
            return await ReadFromCacheAsync<TValue>(cacheKey);
        }

        /// <inheritdoc/>
        public async Task PurgeAsync()
        {
            if (_distributedCache == null)
            {
                return;
            }

            List<string> cacheKeys = await GetCacheKeysAsync();
            foreach (string cacheKey in cacheKeys ?? new List<string>())
            {
                await _distributedCache.RemoveAsync(cacheKey);
            }
            await _distributedCache.RemoveAsync(typeof(TController).Name);
        }

        private string GetCacheKey(HttpRequest request)
        {
            StringBuilder sb = new StringBuilder(typeof(TController).Name);
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

        private async Task<T> ReadFromCacheAsync<T>(string cacheKey)
        {
            byte[] data = await _distributedCache.GetAsync(cacheKey);

            if (data != null && data.Length > 0)
            {
                using var stream = new MemoryStream();
                await stream.WriteAsync(data);
                stream.Position = 0;

                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions);
            }

            return default;
        }

        private async Task AddCacheKeyAsync(string cacheKey)
        {
            List<string> cacheKeys = await ReadFromCacheAsync<List<string>>(typeof(TController).Name);

            if (cacheKeys == null)
            {
                cacheKeys = new List<string>();
            }

            if (cacheKeys.Contains(cacheKey))
            {
                cacheKeys.Add(cacheKey);
                await WriteToCacheAsync(typeof(TController).Name, cacheKeys);
            }
        }

        private async Task<List<string>> GetCacheKeysAsync()
        {
            return await ReadFromCacheAsync<List<string>>(typeof(TController).Name);
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

            await AddCacheKeyAsync(cacheKey);
        }
    }
}
