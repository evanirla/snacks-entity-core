using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IEntityService<TEntity>
        where TEntity : class
    {
        DbContext DbContext { get; }
        DbSet<TEntity> Entities { get; }

        Task<TEntity> FindAsync(params object[] keyValues);

        Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create the given entity and save the changes to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The created entity</returns>
        Task<TEntity> CreateAsync(TEntity model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the given entity and save the changes to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove the given entity and save the changes to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default);
    }

    public interface IEntityService<TEntity, TDbContext> : IEntityService<TEntity>
        where TEntity : class
        where TDbContext : DbContext
    {
        new TDbContext DbContext { get; }
    }
}
