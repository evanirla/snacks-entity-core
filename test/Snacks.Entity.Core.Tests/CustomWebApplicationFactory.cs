using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net.Http;

namespace Snacks.Entity.Core.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartupBase>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder => 
                {
                    builder.UseUrls("http://*:5000; https://*:5001");
                    builder.UseSetting("https_port", "5001");
                    builder.UseEnvironment("Testing");
                    builder.UseStartup<TestStartupBase>();
                });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            return base.CreateHost(builder);
        }
    }
}
