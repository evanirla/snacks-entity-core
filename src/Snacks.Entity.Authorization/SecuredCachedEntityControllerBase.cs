using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Snacks.Entity.Caching;
using Snacks.Entity.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Authorization
{
    public class SecuredCachedEntityControllerBase<TEntity, TKey> : CachedEntityControllerBase<TEntity, TKey>
        where TEntity : class
    {
        protected readonly IAuthorizationService _authorizationService;
        private readonly SecuredGlobalCacheService<TEntity> _securedGlobalCache;

        protected override IEntityCacheService<TEntity> GlobalCache => _securedGlobalCache;

        public SecuredCachedEntityControllerBase(
            IEntityService<TEntity> entityService,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService) : base(entityService, distributedCache, httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _securedGlobalCache = new SecuredGlobalCacheService<TEntity>(distributedCache, httpContextAccessor, authorizationService);
        }

        public override async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            ActionResult<IList<TEntity>> result = await base.GetAsync();

            if (!(result.Result is OkResult))
            {
                return result;
            }

            foreach (TEntity entity in result.Value)
            {
                AuthorizationResult authorizationResult =
                    await _authorizationService.AuthorizeAsync(User, entity, Operations.Read);

                if (!authorizationResult.Succeeded)
                {
                    result.Value.Remove(entity);
                }
            }

            return result;
        }

        public override async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            ActionResult<TEntity> result = await base.GetAsync(id);

            if (!(result.Result is OkResult))
            {
                return result;
            }

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(User, result.Value, Operations.Read);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return result;
        }

        public override async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            ActionResult<TEntity> result = await base.PostAsync(model);

            if (!(result.Result is OkResult))
            {
                return result;
            }

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(User, result.Value, Operations.Create);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return result;
        }

        public override async Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data)
        {
            TEntity entity = await Service.FindAsync(id);

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(User, entity, Operations.Update);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return await base.PatchAsync(id, data);
        }

        public override async Task<IActionResult> DeleteAsync([FromRoute] TKey id)
        {
            TEntity entity = await Service.FindAsync(id);

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(User, entity, Operations.Delete);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return await base.DeleteAsync(id);
        }
    }

    public abstract class SecuredCachedEntityControllerBase<TEntity, TKey, TEntityService> : SecuredCachedEntityControllerBase<TEntity, TKey>, IEntityController<TEntity, TKey, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public SecuredCachedEntityControllerBase(
            TEntityService entityService,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService) : base(entityService, distributedCache, httpContextAccessor, authorizationService)
        {

        }
    }
}
