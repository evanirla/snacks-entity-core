﻿using Microsoft.EntityFrameworkCore;
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
        public void AccessDbContext(Action<TDbContext> dbContextAction)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            dbContextAction.Invoke(dbContext);
        }

        /// <inheritdoc/>
        public TReturn AccessDbContext<TReturn>(Func<TDbContext, TReturn> dbContextFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return dbContextFunc.Invoke(dbContext);
        }

        /// <inheritdoc/>
        public async Task AccessDbContextAsync(Func<TDbContext, Task> dbContextFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbContextFunc.Invoke(dbContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TReturn> AccessDbContextAsync<TReturn>(Func<TDbContext, Task<TReturn>> dbContextFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbContextFunc.Invoke(dbContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void AccessEntities(Action<DbSet<TEntity>> dbSetAction)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            dbSetAction.Invoke(dbContext.Set<TEntity>());
        }

        /// <inheritdoc/>
        public TReturn AccessEntities<TReturn>(Func<DbSet<TEntity>, TReturn> dbSetFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return dbSetFunc.Invoke(dbContext.Set<TEntity>());
        }

        /// <inheritdoc/>
        public async Task AccessEntitiesAsync(Func<DbSet<TEntity>, Task> dbSetFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            await dbSetFunc.Invoke(dbContext.Set<TEntity>()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TReturn> AccessEntitiesAsync<TReturn>(Func<DbSet<TEntity>, Task<TReturn>> dbSetFunc)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            return await dbSetFunc.Invoke(dbContext.Set<TEntity>()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            EntityEntry<TEntity> entry = dbContext.Add(model);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entry.Entity;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            dbContext.Remove(model);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> FindAsync(object key)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = GetDbContext(scope);

            var primaryKey = dbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey();
            var property = primaryKey.Properties.Single();

            return await dbContext.FindAsync<TEntity>(Convert.ChangeType(key, property.ClrType)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
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
