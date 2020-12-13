using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Sqlite;
using System;
using System.Data;
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

        public override async Task<ClassStudent> GetOneAsync(int key, IDbTransaction transaction = null)
        {
            ClassStudent classStudent = await base.GetOneAsync(key, transaction);

            classStudent.Class = await GetOtherEntityService<Class>().GetOneAsync(classStudent.ClassId);
            classStudent.Student = await GetOtherEntityService<Student>().GetOneAsync(classStudent.StudentId);

            return classStudent;
        }
    }
}
