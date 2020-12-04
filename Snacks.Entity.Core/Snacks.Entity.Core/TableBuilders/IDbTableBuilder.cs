using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Snacks.Entity.Core
{
    public interface IDbTableBuilder<TDbService, TDbConnection> 
        where TDbService : IDbService<TDbConnection>
        where TDbConnection : IDbConnection
    {
        
    }
}
