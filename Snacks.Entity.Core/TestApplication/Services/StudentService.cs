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

        public override Task InitializeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
