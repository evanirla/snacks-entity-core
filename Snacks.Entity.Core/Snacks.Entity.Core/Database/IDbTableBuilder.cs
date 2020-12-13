using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    public interface IDbTableBuilder<TDbService> where TDbService : IDbService
    {
        Task CreateTableAsync<T>();
    }
}
