using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Snacks.Entity.Core
{
    /// <inheritdoc cref="IEntityService{TEntity, TDbContext}"/>
    public abstract class EntityServiceBase<TEntity, TDbContext> : IEntityService<TEntity, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityServiceBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task AccessDbContextAsync(Func<TDbContext, Task> dbContextFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbContextFunc.Invoke(dbContext);
        }

        /// <inheritdoc/>
        public async Task<TReturn> AccessDbContextAsync<TReturn>(Func<TDbContext, Task<TReturn>> dbContextFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbContextFunc.Invoke(dbContext);
        }

        /// <inheritdoc/>
        public async Task AccessEntitiesAsync(Func<DbSet<TEntity>, Task> dbSetFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbSetFunc.Invoke(dbContext.Set<TEntity>());
        }

        /// <inheritdoc/>
        public async Task<TReturn> AccessEntitiesAsync<TReturn>(Func<DbSet<TEntity>, Task<TReturn>> dbSetFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbSetFunc.Invoke(dbContext.Set<TEntity>());
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            EntityEntry<TEntity> entry = dbContext.Add(model);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            dbContext.Remove(model);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> FindAsync(object key)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            var primaryKey = dbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey();
            var property = primaryKey.Properties.Single();

            return await dbContext.FindAsync<TEntity>(Convert.ChangeType(key, property.ClrType));
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            EntityEntry<TEntity> entry = dbContext.Update(model);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        private TDbContext GetDbContext(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<TDbContext>();
        }
    }
}
