using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Sqlite.Extensions;
using System;
using TestApplication.Models;

namespace TestApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => 
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });
            services.AddDistributedMemoryCache();

            services.AddSqliteService("Data Source=snacks.db");
            services.AddEntityServices();
            services.AddEntityCacheServices(options => 
            {
                options.EntryAction = entryOptions =>
                {
                    entryOptions.SlidingExpiration = TimeSpan.FromHours(1);
                    entryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                };
            });

            services.AddSwaggerGen(options => 
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseEndpoints(options => 
                {
                    options.MapDefaultControllerRoute();
                })
                .UseSwagger()
                .UseSwaggerUI(options => 
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });

            app.ApplicationServices.GetRequiredService<IEntityService<CustomerModel>>().InitializeAsync();
        }
    }
}
