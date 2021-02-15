using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Snacks.Entity.Caching;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Snacks.Entity.Authorization
{
    class EntityValue<TEntity, TValue>
    {
        public TEntity Entity { get; set; }
        public TValue Value { get; set; }
    }

    public class SecuredGlobalCacheService<TEntity> : EntityCacheServiceBase<TEntity>
        where TEntity : class
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecuredGlobalCacheService(
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService) : base(distributedCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override Task AddValueAsync<TValue>(HttpRequest httpRequest, TEntity model, TValue value, DistributedCacheEntryOptions options = null)
        {
            var entityValue = new EntityValue<TEntity, TValue>
            {
                Entity = model,
                Value = value
            };
            return base.AddValueAsync(httpRequest, model, entityValue, options);
        }

        public override async Task<TEntity> FindAsync(HttpRequest httpRequest)
        {
            TEntity model = await base.FindAsync(httpRequest);

            if (model != null)
            {
                AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(GetUser(), model, Operations.Read);

                if (authorizationResult.Succeeded)
                {
                    return model;
                }
            }

            return default;
        }

        public override async Task<IList<TEntity>> GetAsync(HttpRequest httpRequest)
        {
            IList<TEntity> models = await base.GetAsync(httpRequest);

            if (models != null)
            {
                foreach (TEntity model in models)
                {
                    AuthorizationResult authorizationResult =
                        await _authorizationService.AuthorizeAsync(GetUser(), model, Operations.Read);

                    if (!authorizationResult.Succeeded)
                    {
                        break;
                    }
                }
            }

            return default;
        }

        public override async Task<TValue> GetValueAsync<TValue>(HttpRequest request, TEntity model)
        {
            string cacheKey = GetCacheKey(request);
            EntityValue<TEntity, TValue> entityValue = await ReadFromCacheAsync<EntityValue<TEntity, TValue>>(cacheKey).ConfigureAwait(false);

            if (entityValue != null)
            {
                AuthorizationResult authorizationResult =
                    await _authorizationService.AuthorizeAsync(GetUser(), entityValue.Entity, Operations.Read);

                if (authorizationResult.Succeeded)
                {
                    return entityValue.Value;
                }
            }

            return default;
        }

        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor.HttpContext.User;
        }
    }
}
