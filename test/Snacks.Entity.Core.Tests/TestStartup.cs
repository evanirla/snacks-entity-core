using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Tests.Server.Database;

namespace Snacks.Entity.Core.Tests
{
    public class TestStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(options => { options.MapDefaultControllerRoute(); });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<GlobalDbContext>(options => 
            {
                options.UseInMemoryDatabase("SnacksDb");
            });

            services
                .AddDistributedMemoryCache()
                .AddEntityServices()
                .AddControllers();
            
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
        }
    }
}
