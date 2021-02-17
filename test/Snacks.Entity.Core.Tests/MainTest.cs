using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Snacks.Entity.Core.Tests
{
    public class MainTest : TestBase
    {
        [Fact]
        public async void GetCustomers()
        {
            using (HttpClient client = GetClient())
            {
                HttpResponseMessage message = await client.GetAsync("customers");

                Assert.Equal(HttpStatusCode.OK, message.StatusCode);
            }
        }
    }
}
