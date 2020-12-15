using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    public abstract class BaseTableBuilder<TDbService> : IDbTableBuilder<TDbService>
        where TDbService : IDbService
    {
        protected readonly IDbService _dbService;

        public BaseTableBuilder(IDbService dbService)
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
