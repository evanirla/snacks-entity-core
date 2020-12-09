using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class ClassStudentService : BaseEntityService<ClassStudent, int, SqliteService, SqliteConnection>
    {
        public ClassStudentService(
            IServiceProvider serviceProvider,
            ILogger<ClassStudentService> logger) : base(serviceProvider, logger)
        {
            
        }

        public override async Task InitializeAsync()
        {
            SqliteTableBuilder sqliteTableBuilder = new SqliteTableBuilder(_dbService);
            await sqliteTableBuilder.CreateTableAsync<ClassStudent>();
        }
    }
}
