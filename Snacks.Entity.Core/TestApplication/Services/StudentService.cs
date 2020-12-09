using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class StudentService : BaseEntityService<Student, int, SqliteService, SqliteConnection>
    {
        public StudentService(
            IServiceProvider serviceProvider,
            ILogger<StudentService> logger) : base(serviceProvider, logger)
        {
            
        }

        public override async Task InitializeAsync()
        {
            SqliteTableBuilder sqliteTableBuilder = new SqliteTableBuilder(_dbService);
            await sqliteTableBuilder.CreateTableAsync<Student>();
        }
    }
}
