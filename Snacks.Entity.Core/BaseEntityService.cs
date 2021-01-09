using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class BaseEntityService<TEntity, TDbContext> : IEntityService<TEntity, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        public TDbContext DbContext => _dbContext;
        public DbSet<TEntity> Entities => _dbContext.Set<TEntity>();

        protected readonly TDbContext _dbContext;

        public BaseEntityService(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            EntityEntry<TEntity> entry = await _dbContext.AddAsync(model, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        public async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            _dbContext.Remove(model);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbContext.FindAsync<TEntity>(keyValues).ConfigureAwait(false);
        }

        public async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            return await _dbContext.FindAsync<TEntity>(keyValues, cancellationToken).ConfigureAwait(false);
        }

        public IQueryable<TEntity> Query()
        {
            return _dbContext.Set<TEntity>();
        }

        public async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            EntityEntry<TEntity> entry = _dbContext.Update(model);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }
    }
}
