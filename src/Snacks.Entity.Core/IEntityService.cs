using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Provides CRUD operations for <typeparamref name="TEntity"/>.
    /// </summary>
    /// <remarks>
    /// Not intended to be implemented, see <see cref="IEntityService{TEntity, TDbContext}"/>.
    /// </remarks>
    /// <typeparam name="TEntity">An entity model type</typeparam>
    public interface IEntityService<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Asynchronously return the entity with the given key values.
        /// </summary>
        /// <param name="keyValues">The key values that make up the primary key</param>
        Task<TEntity> FindAsync(params object[] keyValues);

        /// <summary>
        /// Asynchronously create the given <typeparamref name="TEntity"/> and save the changes to the database.
        /// </summary>
        /// <param name="model">An instance of <typeparamref name="TEntity"/></param>
        Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously update the given <typeparamref name="TEntity"/> and save the changes to the database.
        /// </summary>
        /// <param name="model">An instance of <typeparamref name="TEntity"/></param>
        Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously remove the given <typeparamref name="TEntity"/> and save the changes to the database.
        /// </summary>
        /// <param name="model">An instance of <typeparamref name="TEntity"/></param>
        Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Provide synchronous read access to the <see cref="DbSet{}"/> of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// entityService.AccessEntities(entities => {
        ///     entities.Where(e => e.Name == "Entity1").ToList();
        /// });
        /// </code>
        /// </example>
        /// <remarks>
        /// If update access is needed, use <see cref="IEntityService{TEntity, TDbContext}.AccessDbContextAsync(Func{TDbContext, Task})"/> instead.
        /// </remarks>
        /// <param name="dbSetAction">An action to perform against the <see cref="DbSet{}"/> of <typeparamref name="TEntity"/></param>
        void AccessEntities(Action<DbSet<TEntity>> dbSetAction);

        /// <summary>
        /// Provide asynchronous read access to the <see cref="DbSet{}"/> of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// await entityService.AccessEntitiesAsync(async entities => {
        ///     await entities.Where(e => e.Name == "Entity1").ToListAsync();
        /// });
        /// </code>
        /// </example>
        /// <remarks>
        /// If update access is needed, use <see cref="IEntityService{TEntity, TDbContext}.AccessDbContextAsync(Func{TDbContext, Task})"/> instead.
        /// </remarks>
        /// <param name="dbSetFunc">An asynchronous action to perform against the <see cref="DbSet{}"/> of <typeparamref name="TEntity"/></param>
        Task AccessEntitiesAsync(Func<DbSet<TEntity>, Task> dbSetFunc);
    }

    /// <summary>
    /// Provides CRUD operations for <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">An entity model type</typeparam>
    /// <typeparam name="TDbContext">An implementation of <see cref="DbContext"/> that contains a <see cref="DbSet{}"/> of <typeparamref name="TEntity"/></typeparam>
    public interface IEntityService<TEntity, TDbContext> : IEntityService<TEntity>
        where TEntity : class
        where TDbContext : DbContext
    {
        /// <summary>
        /// Provide synchronous read/update access to a scoped instance of <typeparamref name="TDbContext"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// entityService.AccessDbContext(dbContext => {
        ///     dbContext.AddRange(entities);
        ///     dbContext.SaveChanges();
        /// });
        /// </code>
        /// </example>
        /// <remarks>
        /// Use this method if you need to perform more than one CRUD operation against a scoped instance of <typeparamref name="TDbContext"/> within the same transaction.
        /// </remarks>
        /// <param name="dbContextAction">An action to perform against a scoped instance of <typeparamref name="TDbContext"/></param>
        void AccessDbContext(Action<TDbContext> dbContextAction);

        /// <summary>
        /// Provide asynchronous read/update access to a scoped instance of <typeparamref name="TDbContext"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// await entityService.AccessDbContextAsync(dbContext => {
        ///     dbContext.AddRange(entities);
        ///     await dbContext.SaveChangesAsync();
        /// });
        /// </code>
        /// </example>
        /// <remarks>
        /// Use this method if you need to perform more than one CRUD operation against a scoped instance of <typeparamref name="TDbContext"/> within the same transaction.
        /// </remarks>
        /// <param name="dbContextAction">An asynchronous action to perform against a scoped instance of <typeparamref name="TDbContext"/></param>
        Task AccessDbContextAsync(Func<TDbContext, Task> dbContextFunc);
    }
}
