using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    public interface IDbTableBuilder<TDbService, TDbConnection> 
        where TDbService : IDbService<TDbConnection>
        where TDbConnection : IDbConnection
    {
        Task CreateTableAsync<T>();
    }
}
