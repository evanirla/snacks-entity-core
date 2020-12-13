using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Database;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Entity
{
    public interface IEntityService
    {
        Task<IEntityModel> GetOneAsync(object key, IDbTransaction transaction = null);

        Task<IEnumerable<IEntityModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null);

        Task<IEnumerable<IEntityModel>> GetManyAsync(string sql, object parameters, IDbTransaction transaction = null);

        Task<IEntityModel> CreateOneAsync(IEntityModel model, IDbTransaction transaction = null);

        Task<IEnumerable<IEntityModel>> CreateManyAsync(IEnumerable<IEntityModel> models, IDbTransaction transaction = null);

        Task UpdateOneAsync(IEntityModel model, IDbTransaction transaction = null);

        Task DeleteOneAsync(object key, IDbTransaction transaction = null);

        Task DeleteOneAsync(IEntityModel model, IDbTransaction transaction = null);

        Task InitializeAsync();
    }

    public interface IEntityService<TModel> : IEntityService
        where TModel : IEntityModel
    {
        new Task<TModel> GetOneAsync(object key, IDbTransaction transaction = null);

        new Task<IEnumerable<TModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null);

        new Task<IEnumerable<TModel>> GetManyAsync(string sql, object parameters, IDbTransaction transaction = null);

        Task<TModel> CreateOneAsync(TModel model, IDbTransaction transaction = null);

        Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models, IDbTransaction transaction = null);

        Task UpdateOneAsync(TModel model, IDbTransaction transaction = null);

        Task DeleteOneAsync(TModel model, IDbTransaction transaction = null);
    }

    public interface IEntityService<TModel, TDbService> : IEntityService<TModel>
        where TModel : IEntityModel
        where TDbService : IDbService
    {

    }
}
