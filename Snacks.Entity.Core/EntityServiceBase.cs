using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Wraps the given DbContext into an entity-specific service for data retrieval and manipulation.
    /// The DbContext and Entity DbSet can be accessed by using AccessDbContext or AccessEntities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
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

        public async Task AccessDbContextAsync(Func<TDbContext, Task> dbContextFunc)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbContextFunc.Invoke(dbContext).ConfigureAwait(false);
        }

        public void AccessEntities(Action<DbSet<TEntity>> dbSetAction)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            dbSetAction.Invoke(dbContext.Set<TEntity>());
        }

        public async Task AccessEntitiesAsync(Func<DbSet<TEntity>, Task> dbSetFunc)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbSetFunc.Invoke(dbContext.Set<TEntity>()).ConfigureAwait(false);
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

        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbContext.FindAsync<TEntity>(keyValues).ConfigureAwait(false);
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
