using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IDistributedCacheHelper<TController> where TController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        Task AddRequestAsync<TValue>(HttpRequest httpRequest, TValue value, DistributedCacheEntryOptions options = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        Task<TValue> GetFromRequestAsync<TValue>(HttpRequest httpRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        Task PurgeAsync();
    }
}
