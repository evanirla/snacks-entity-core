using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Data;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class ClassStudentService : BaseEntityService<ClassStudent, int, SqliteService, SqliteConnection>
    {
        readonly IEntityService<Class> _classService;
        readonly IEntityService<Student> _studentService;

        public ClassStudentService(
            IServiceProvider serviceProvider,
            ILogger<ClassStudentService> logger,
            IEntityService<Class> classService,
            IEntityService<Student> studentService) : base(serviceProvider, logger)
        {
            _classService = classService;
            _studentService = studentService;
        }

        public override async Task InitializeAsync()
        {
            SqliteTableBuilder sqliteTableBuilder = new SqliteTableBuilder(_dbService);
            await sqliteTableBuilder.CreateTableAsync<ClassStudent>();
        }

        public override async Task<ClassStudent> GetOneAsync(int key, IDbTransaction transaction = null)
        {
            ClassStudent classStudent = await base.GetOneAsync(key, transaction);

            classStudent.Class = await _classService.GetOneAsync(classStudent.ClassId);
            classStudent.Student = await _studentService.GetOneAsync(classStudent.StudentId);

            return classStudent;
        }
    }
}
