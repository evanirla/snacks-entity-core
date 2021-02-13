using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Implemented by base classes to handle GET, POST, PATCH, and DELETE web requests for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for the controller</typeparam>
    /// <typeparam name="TKey">A primitive type that matches the key type of the entity</typeparam>
    public interface IEntityController<TEntity, TKey>
        where TEntity : class
    {
        Task<IActionResult> DeleteAsync([FromRoute] TKey id);
        Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id);
        Task<ActionResult<IList<TEntity>>> GetAsync();
        Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model);
        Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data);
    }

    /// <inheritdoc/>
    /// <typeparam name="TEntityService">The service for the entity</typeparam>
    public interface IEntityController<TEntity, TKey, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        
    }
}
