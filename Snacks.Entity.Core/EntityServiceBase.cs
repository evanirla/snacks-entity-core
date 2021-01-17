using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Snacks.Entity.Core
{
    public abstract class EntityServiceBase<TEntity, TDbContext> : IEntityService<TEntity, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        protected IServiceScopeFactory _scopeFactory;
        

        public EntityServiceBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void AccessDbContext(Action<TDbContext> dbContextAction)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            dbContextAction.Invoke(dbContext);
        }

        public Task AccessDbContextAsync(Func<TDbContext, Task> dbContextFunc)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            return dbContextFunc.Invoke(dbContext);
        }

        public void AccessEntities(Action<DbSet<TEntity>> dbSetAction)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            dbSetAction.Invoke(dbContext.Set<TEntity>());
        }

        public Task AccessEntitiesAsync(Func<DbSet<TEntity>, Task> dbSetFunc)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            return dbSetFunc.Invoke(dbContext.Set<TEntity>());
        }

        public virtual async Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            EntityEntry<TEntity> entry = dbContext.Add(model);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            dbContext.Remove(model);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await FindAsync(keyValues, default).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbContext.FindAsync<TEntity>(keyValues, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            EntityEntry<TEntity> entry = dbContext.Update(model);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        private TDbContext GetDbContext(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<TDbContext>();
        }
    }
}
