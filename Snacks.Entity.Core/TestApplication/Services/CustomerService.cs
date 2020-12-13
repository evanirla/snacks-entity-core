using Snacks.Entity.Core.Entity;
using Snacks.Entity.Core.Sqlite;
using System;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Services
{
    public class CustomerService : BaseEntityService<CustomerModel, SqliteService>
    {
        public CustomerService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            SqliteTableBuilder tableBuilder = new SqliteTableBuilder(_dbService);
            await tableBuilder.CreateTableAsync<CustomerModel>();
        }
    }
}
