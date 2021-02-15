using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Snacks.Entity.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snacks.Entity.Caching
{
    public class CachedEntityControllerBase<TEntity, TKey> : EntityControllerBase<TEntity, TKey>
        where TEntity : class
    {
        protected virtual IEntityCacheService<TEntity> GlobalCache { get; private set; }
        protected UserCacheService<TEntity> UserCache { get; private set; }

        public CachedEntityControllerBase(
            IEntityService<TEntity> entityService,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor) : base(entityService)
        {
            // is there any performance degradation here since controllers are scoped?
            GlobalCache = new GlobalCacheService<TEntity>(distributedCache);
            UserCache = new UserCacheService<TEntity>(distributedCache, httpContextAccessor);
        }

        public override async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            TEntity model = await GlobalCache.FindAsync(Request).ConfigureAwait(false);
            if (model != null)
            {
                return model;
            }

            var result = await base.GetAsync(id).ConfigureAwait(false);

            if (result.Result is OkResult)
            {
                await GlobalCache.AddAsync(Request, result.Value).ConfigureAwait(false);
            }

            return result;
        }

        public override async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            IList<TEntity> cachedModels = await GlobalCache.GetAsync(Request).ConfigureAwait(false);

            if (cachedModels != null)
            {
                return cachedModels.ToList();
            }

            var result = await base.GetAsync().ConfigureAwait(false);

            if (result.Result is OkResult)
            {
                await GlobalCache.AddAsync(Request, result.Value).ConfigureAwait(false);
            }

            return result;
        }

        public override async Task<IActionResult> DeleteAsync([FromRoute] TKey id)
        {
            var result = await base.DeleteAsync(id).ConfigureAwait(false);

            if (result is OkResult)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return result;
        }

        public override async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            var result = await base.PostAsync(model).ConfigureAwait(false);

            if (result.Result is OkResult)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return result;
        }

        public override async Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data)
        {
            var result = await base.PatchAsync(id, data);

            if (result is OkResult)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return result;
        }
    }

    public class CachedEntityControllerBase<TEntity, TKey, TEntityService> : CachedEntityControllerBase<TEntity, TKey>, IEntityController<TEntity, TKey, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public CachedEntityControllerBase(
            TEntityService entityService,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor) : base(
                entityService, 
                distributedCache, 
                httpContextAccessor)
        {

        }
    }
}
