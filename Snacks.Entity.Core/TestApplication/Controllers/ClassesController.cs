using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Snacks.Entity.Core.Controllers;
using Snacks.Entity.Core.Database;
using TestApplication.Models;
using TestApplication.Services;

namespace TestApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : BaseEntityController<Class, int, SqliteService, SqliteConnection>
    {
        public ClassesController(ClassService classService) : base(classService)
        {
            
        }
    }
}
