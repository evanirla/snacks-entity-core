using Snacks.Entity.Core.Tests.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Snacks.Entity.Core.Tests
{
    [Collection("Core Collection")]
    public class CoreTest : TestBase
    {
        const int CUSTOMER_COUNT = 100;
        const int ITEM_COUNT = 10;

        private bool testDataCreated;

        [Fact(DisplayName = "Create Test Data")]
        public async Task CreateTestDataAsync()
        {
            using (HttpClient client = GetClient())
            {
                await Task.WhenAll(CreateItemsAsync(client), CreateCustomersAsync(client));
                await CreateCartsAsync(client);
                testDataCreated = true;
            }
        }

        [Fact(DisplayName = "Query Test Data")]
        public async Task QueryTestDataAsync()
        {
            if (!testDataCreated)
            {
                await CreateTestDataAsync();
            }

            using (HttpClient client = GetClient())
            {
                var response = await client.GetAsync("customers?name=Customer 1");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                List<CustomerModel> customers = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
                Assert.Single(customers);
            }
        }

        private async Task CreateItemsAsync(HttpClient client)
        {
            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < ITEM_COUNT; ++i)
            {
                tasks.Add(client.PostAsJsonAsync("items", new ItemModel
                {
                    Name = $"Item {i + 1}",
                    Price = Convert.ToDecimal(i) + Convert.ToDecimal(0.99)
                }));
            }

            var responses = await Task.WhenAll(tasks);
            Assert.All(responses, x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        }

        private async Task CreateCustomersAsync(HttpClient client)
        {
            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 1; i <= CUSTOMER_COUNT; ++i)
            {
                tasks.Add(client.PostAsJsonAsync("customers", new CustomerModel
                {
                    Name = $"Customer {i}"
                }));
            }

            var responses = await Task.WhenAll(tasks);
            Assert.All(responses, x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        }

        private async Task CreateCartsAsync(HttpClient client)
        {
            HttpResponseMessage getCustomersResponse = await client.GetAsync("customers");
            Assert.Equal(HttpStatusCode.OK, getCustomersResponse.StatusCode);

            List<CustomerModel> customers = await GetAllCustomersAsync(client);
            List<ItemModel> items = await GetAllItemsAsync(client);
            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

            foreach (CustomerModel customer in customers)
            {
                tasks.Add(client.PostAsJsonAsync("carts", new CartModel
                {
                    CustomerId = customer.Id,
                    Items = items.Select(x => new CartItemModel { ItemId = x.Id }).ToList()
                }));
            }

            var responses = await Task.WhenAll(tasks);
            Assert.All(responses, x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        }

        private async Task<List<CustomerModel>> GetAllCustomersAsync(HttpClient client)
        {
            HttpResponseMessage getCustomersResponse = await client.GetAsync("customers");
            Assert.Equal(HttpStatusCode.OK, getCustomersResponse.StatusCode);

            return await getCustomersResponse.Content.ReadFromJsonAsync<List<CustomerModel>>();
        }

        private async Task<List<ItemModel>> GetAllItemsAsync(HttpClient client)
        {
            HttpResponseMessage getItemsResponse = await client.GetAsync("items");
            Assert.Equal(HttpStatusCode.OK, getItemsResponse.StatusCode);

            return await getItemsResponse.Content.ReadFromJsonAsync<List<ItemModel>>();
        }


    }
}
