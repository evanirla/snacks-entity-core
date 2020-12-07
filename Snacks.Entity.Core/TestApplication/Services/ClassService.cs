using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class ClassService : BaseEntityService<Class, int, SqliteService, SqliteConnection>
    {
        public ClassService(
            IServiceProvider serviceProvider,
            ILogger<ClassService> logger) : base(serviceProvider, logger)
        {
            
        }

        public override async Task InitializeAsync()
        {
            SqliteTableBuilder sqliteTableBuilder = new SqliteTableBuilder(_dbService);
            await sqliteTableBuilder.CreateTableAsync<Class>();

            await CreateManyAsync(new List<Class>
            {
                new Class
                {
                    Name = "Class 1"
                },
                new Class
                {
                    Name = "Class 2"
                },
                new Class
                {
                    Name = "Class 3"
                }
            });
        }
    }
}
