using Microsoft.AspNetCore.Http;
using Snacks.Entity.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Caching
{
    public interface IEntityCacheService<TModel>
        where TModel : IEntityModel
    {
        Task<TModel> GetCustomOneAsync(string cacheKey);

        Task<IList<TModel>> GetCustomManyAsync(string cacheKey);

        Task<TModel> GetOneAsync(object key);

        Task<IList<TModel>> GetManyAsync(IQueryCollection queryCollection);

        Task SetCustomOneAsync(string cacheKey, TModel model);

        Task SetCustomManyAsync(string cacheKey, IList<TModel> models);

        Task SetOneAsync(TModel model);

        Task SetManyAsync(IQueryCollection queryCollection, IList<TModel> models);

        Task RemoveOneAsync(TModel model);

        Task RemoveOneAsync(object key);

        Task RemoveManyAsync(IQueryCollection queryCollection = null);
    }

    public interface IEntityCacheService<TModel, TKey> : IEntityCacheService<TModel>
        where TModel : IEntityModel<TKey>
    {
        Task<TModel> GetOneAsync(TKey key);

        Task RemoveOneAsync(TKey key);
    }
}
