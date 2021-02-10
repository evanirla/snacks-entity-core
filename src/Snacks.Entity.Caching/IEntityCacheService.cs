using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Caching
{
    public interface IEntityCacheService<TEntity>
        where TEntity : class
    {
        Task AddAsync(HttpRequest httpRequest, TEntity model, DistributedCacheEntryOptions options = null);
        Task AddAsync(HttpRequest httpRequest, IList<TEntity> models, DistributedCacheEntryOptions options = null);
        Task AddRelatedAsync<TRelated>(HttpRequest httpRequest, IList<TRelated> relatedModels, DistributedCacheEntryOptions options = null) where TRelated : class;
        Task AddValueAsync<TValue>(HttpRequest httpRequest, TEntity model, TValue value, DistributedCacheEntryOptions options = null);
        Task<TEntity> FindAsync(HttpRequest httpRequest);
        Task<IList<TEntity>> GetAsync(HttpRequest httpRequest);
        Task<IList<TRelated>> GetRelatedAsync<TRelated>(HttpRequest httpRequest) where TRelated : class;
        Task<TValue> GetValueAsync<TValue>(HttpRequest httpRequest, TEntity model);
        Task PurgeAsync();
    }
}
