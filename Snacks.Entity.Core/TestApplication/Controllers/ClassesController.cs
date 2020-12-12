using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Controllers;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using TestApplication.Models;
using TestApplication.Services;

namespace TestApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : BaseEntityController<Class, int, SqliteService, SqliteConnection>
    {
        ClassService EntityService => (ClassService)_entityService;

        public ClassesController(IEntityService<Class> classService) : base((ClassService)classService)
        {
            
        }

        public override async Task<IActionResult> GetAsync()
        {
            await _entityService.CreateManyAsync(new List<Class>
            {
                new Class
                {
                    Name = "Math 101",
                    Students = new List<ClassStudent>
                    {
                        new ClassStudent
                        {
                            Student = new Student
                            {
                                Name = "Jase Markerton"
                            }
                        }
                    }
                }
            });

            return await base.GetAsync();
        }

        [HttpGet("{key}/students")]
        public async Task<IActionResult> GetStudentsAsync(int key)
        {
            var students = await EntityService.GetStudentsAsync(key);
            return new JsonResult(students);
        }
    }
}
