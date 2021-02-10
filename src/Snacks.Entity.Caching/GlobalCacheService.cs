using Microsoft.Extensions.Caching.Distributed;

namespace Snacks.Entity.Caching
{
    public class GlobalCacheService<TEntity> : EntityCacheServiceBase<TEntity>
        where TEntity : class
    {
        public GlobalCacheService(
            IDistributedCache distributedCache) : base(distributedCache)
        {
            
        }
    }
}
