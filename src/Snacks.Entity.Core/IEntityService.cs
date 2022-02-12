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
    /// Please implement <see cref="IEntityService{TEntity, TDbContext}"/> instead.
    /// </remarks>
    /// <typeparam name="TEntity">An entity model type</typeparam>
    public interface IEntityService<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Asynchronously return the entity with the given key. Models with composite keys
        /// are not supported.
        /// </summary>
        /// <param name="key">The key value of the target entity</param>
        Task<TEntity> FindAsync(object key);

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

        Task<TReturn> AccessEntitiesAsync<TReturn>(Func<DbSet<TEntity>, Task<TReturn>> dbSetFunc);
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

        Task<TReturn> AccessDbContextAsync<TReturn>(Func<TDbContext, Task<TReturn>> dbContextFunc);
    }
}
