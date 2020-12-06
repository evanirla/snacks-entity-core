using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    public abstract class BaseTableBuilder<TDbService, TDbConnection> :
        IDbTableBuilder<TDbService, TDbConnection>
        where TDbService : IDbService<TDbConnection>
        where TDbConnection : IDbConnection
    {
        protected readonly IDbService<TDbConnection> _dbService;

        public BaseTableBuilder(IDbService<TDbConnection> dbService)
        {
            _dbService = dbService;
        }

        public abstract Task CreateTableAsync<T>();

        protected TableMapping GetTableMapping<T>()
        {
            return TableMapping.GetMapping<T>();
        }
    }
}
