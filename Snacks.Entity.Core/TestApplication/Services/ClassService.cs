using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class ClassService : BaseEntityService<Class, int, SqliteService, SqliteConnection>
    {
        private readonly ClassStudentService _classStudentService;
        private readonly StudentService _studentService;

        public ClassService(
            IServiceProvider serviceProvider,
            ILogger<ClassService> logger,
            IEntityService<ClassStudent> classStudentService,
            IEntityService<Student> studentService) : base(serviceProvider, logger)
        {
            _classStudentService = (ClassStudentService)classStudentService;
            _studentService = (StudentService)studentService;
        }

        public override async Task InitializeAsync()
        {
            SqliteTableBuilder sqliteTableBuilder = new SqliteTableBuilder(_dbService);
            await sqliteTableBuilder.CreateTableAsync<Class>();
        }

        public override async Task<Class> CreateOneAsync(Class model, IDbTransaction transaction = null)
        {
            async Task<Class> createOne()
            {
                Class newModel = await base.CreateOneAsync(model, transaction);

                foreach (ClassStudent classStudent in model.Students)
                {
                    classStudent.ClassId = newModel.ClassId;

                    if (classStudent.Student != null)
                    {
                        if (classStudent.Student.StudentId != default)
                        {
                            classStudent.StudentId = classStudent.Student.StudentId;
                        }
                        else
                        {
                            classStudent.Student = await _studentService.CreateOneAsync(classStudent.Student, transaction);
                            classStudent.StudentId = classStudent.Student.StudentId;
                        }
                    }

                    await _classStudentService.CreateOneAsync(classStudent, transaction);
                }

                return newModel;
            }

            if (transaction == null)
            {
                using SqliteConnection connection = await _dbService.GetConnectionAsync();
                transaction = connection.BeginTransaction();
                Class newModel = await createOne();
                transaction.Commit();
                return newModel;
            }
            else
            {
                return await createOne();
            }
        }

        public async Task<IEnumerable<Student>> GetStudentsAsync(int key, IDbTransaction transaction = null)
        {
            Class @class = await GetOneAsync(key);

            if (@class == null)
            {
                return default;
            }

            IEnumerable<ClassStudent> classStudents = await _classStudentService.GetManyAsync(@$"
                select StudentId
                from ClassStudent
                where ClassId = @Key", @class, transaction);

            List<Student> students = new List<Student>();
            foreach (ClassStudent classStudent in classStudents)
            {
                Student student = await _studentService.GetOneAsync(classStudent.StudentId);
                students.Add(student);
            }

            return students;
        }
    }
}
