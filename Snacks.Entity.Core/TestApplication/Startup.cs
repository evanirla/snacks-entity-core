using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Sqlite.Extensions;
using TestApplication.Models;

namespace TestApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDistributedMemoryCache();

            services.AddSqliteService("Data Source=snacks.db");
            services.AddEntityServices();
            services.AddEntityCacheServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(options => 
            {
                options.MapDefaultControllerRoute();
            });

            app.ApplicationServices.GetRequiredService<IEntityService<CustomerModel>>().InitializeAsync();
        }
    }
}
