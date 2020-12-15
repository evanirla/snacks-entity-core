using Microsoft.AspNetCore.Http;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class OtherModelService : IEntityService<OtherModel>
    {
        public TableMapping Mapping => throw new NotImplementedException();

        public Task<IEnumerable<OtherModel>> CreateManyAsync(IEnumerable<OtherModel> models, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IEntityModel>> CreateManyAsync(IEnumerable<IEntityModel> models, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<OtherModel> CreateOneAsync(OtherModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEntityModel> CreateOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOneAsync(OtherModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOneAsync(object key, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OtherModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OtherModel>> GetManyAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task<OtherModel> GetOneAsync(object key, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateOneAsync(OtherModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(string sql, object parameters, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        Task<IEntityModel> IEntityService.GetOneAsync(object key, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
