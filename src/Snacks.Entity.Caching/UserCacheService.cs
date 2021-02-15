using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Snacks.Entity.Caching
{
    public class UserCacheService<TEntity> : EntityCacheServiceBase<TEntity>
        where TEntity : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserCacheService(
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor) : base(distributedCache)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override string GetCacheKey(HttpRequest request)
        {
            string cacheKey = base.GetCacheKey(request);

            var user = _httpContextAccessor.HttpContext.User;

            if (user != null)
            {
                return user.Identity.Name + cacheKey;
            }
            else
            {
                return "Public" + cacheKey;
            }
        }
    }
}
