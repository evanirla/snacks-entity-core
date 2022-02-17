using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using Xunit;

namespace Snacks.Entity.Core.Tests
{
    public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>
    {
        public const string BASE_URI = "http://localhost:5000/api/";

        protected readonly static CustomWebApplicationFactory _appFactory;

        static TestBase()
        {
            _appFactory = new CustomWebApplicationFactory();
        }

        protected virtual HttpClient GetClient()
        {
            return _appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri(BASE_URI)
            });
        }
    }
}
