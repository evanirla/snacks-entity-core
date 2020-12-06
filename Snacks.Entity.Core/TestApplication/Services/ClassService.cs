using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
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
    }
}
