using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Snacks.Entity.Core.Tests.Server.Models;
using Xunit;

namespace Snacks.Entity.Core.Tests
{
    public abstract class EntityTestBase<TEntity> : TestBase where TEntity : class
    {
        protected abstract string RelativeUri { get; }
        protected abstract TEntity CreateTemplate { get; }
        protected abstract IList<TEntity> CreateManyTemplate { get; }

        public EntityTestBase()
        {

        }

        [Fact(DisplayName = "Create One")]
        public async Task TestCreateOneAsync()
        {
            using HttpClient client = GetClient();
            var response = await client.PostAsJsonAsync(RelativeUri, CreateTemplate);
            ValidateResponse(response);
        }

        [Fact(DisplayName = "Create Many")]
        public async Task TestCreateManyAsync()
        {
            using HttpClient client = GetClient();
            var response = await client.PostAsJsonAsync(RelativeUri, CreateManyTemplate);
            ValidateResponse(response);
        }

        [Fact(DisplayName = "Delete One")]
        public async Task TestDeleteOneAsync()
        {
            using HttpClient client = GetClient();
            IList<dynamic> entities = await client.GetFromJsonAsync<IList<dynamic>>(RelativeUri);
            var response = await client.DeleteAsync(new Uri(new Uri(RelativeUri), entities.First().Id));
            ValidateResponse(response);
        }

        protected abstract Task DeleteOneAsync(HttpClient client);
        protected abstract Task<TEntity> GetOneAsync(string id);
        protected abstract Task<IList<TEntity>> GetManyAsync();
        protected abstract Task<IList<TEntity>> GetAllAsync();
        protected abstract Task<TEntity> UpdateOneAsync(HttpClient client);

        private void ValidateResponse(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
