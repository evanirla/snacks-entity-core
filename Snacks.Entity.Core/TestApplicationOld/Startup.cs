using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Snacks.Entity.Core.Caching;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Sqlite;
using System.Threading.Tasks;
using TestApplication.Models;
using TestApplication.Services;

namespace TestApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDistributedMemoryCache();

            services.AddSingleton<IDbService<SqliteConnection>>(new SqliteService("Data Source=snacks.db"));
            services.AddSingleton<IEntityCacheService<Class>, EntityCacheService<Class>>();
            services.AddSingleton<IEntityService<Class>, ClassService>();
            services.AddSingleton<IEntityService<Student>, StudentService>();
            services.AddSingleton<IEntityService<ClassStudent>, ClassStudentService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Task.WaitAll(
                app.ApplicationServices.GetService<IEntityService<Class>>().InitializeAsync(),
                app.ApplicationServices.GetService<IEntityService<Student>>().InitializeAsync(),
                app.ApplicationServices.GetService<IEntityService<ClassStudent>>().InitializeAsync());
        }
    }
}
