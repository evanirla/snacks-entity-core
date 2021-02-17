using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using Xunit;

namespace Snacks.Entity.Core.Tests
{
    public class TestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly static CustomWebApplicationFactory _appFactory;

        static TestBase()
        {
            _appFactory = new CustomWebApplicationFactory();
        }

        protected HttpClient GetClient()
        {
            return _appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:5000/api/")
            });
        }
    }
}
