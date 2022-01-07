using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IDistributedCacheHelper<TController> where TController : ControllerBase
    {
        /// <summary>
        /// Asynchronously adds the response returned from an HTTP request to the cache
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        Task AddRequestAsync<TValue>(HttpRequest httpRequest, TValue value, DistributedCacheEntryOptions options = null);

        /// <summary>
        /// Asynchronously returns the response from a previous HTTP request from the cache
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        Task<TValue> GetFromRequestAsync<TValue>(HttpRequest httpRequest);

        /// <summary>
        /// Purges all cache for <see cref="TController" />
        /// </summary>
        /// <returns></returns>
        Task PurgeAsync();
    }
}
