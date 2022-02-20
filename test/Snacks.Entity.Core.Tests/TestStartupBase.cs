using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Tests.Server.Database;

namespace Snacks.Entity.Core.Tests
{
    public class TestStartupBase
    {
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(options =>
                {
                    options.MapDefaultControllerRoute();
                });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<GlobalDbContext>(options => 
            {
                options.UseInMemoryDatabase("SnacksDb");
            });

            services.AddEntityProvider<GlobalDbContext>();

            services
                .AddControllers();
            
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
        }
    }
}
