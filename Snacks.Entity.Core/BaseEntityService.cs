using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class BaseEntityService<TEntity, TDbContext> : IEntityService<TEntity, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        public TDbContext DbContext => _dbContext;
        DbContext IEntityService<TEntity>.DbContext => DbContext;
        
        public DbSet<TEntity> Entities => _dbContext.Set<TEntity>();

        protected readonly TDbContext _dbContext;

        public BaseEntityService(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            EntityEntry<TEntity> entry = _dbContext.Add(model);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            _dbContext.Remove(model);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await FindAsync(keyValues, default).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            return await _dbContext.FindAsync<TEntity>(keyValues, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            EntityEntry<TEntity> entry = _dbContext.Update(model);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }
    }
}
