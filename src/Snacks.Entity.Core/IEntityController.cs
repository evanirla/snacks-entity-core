using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Handles GET, POST, PATCH, and DELETE requests for <typeparamref name="TEntity"/>s
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle requests for</typeparam>
    public interface IEntityController<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Deletes the <typeparamref name="TEntity"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the targeted <typeparamref name="TEntity"/></param>
        Task<IActionResult> DeleteAsync([FromRoute] string id);

        /// <summary>
        /// Return the <typeparamref name="TEntity"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the targeted <typeparamref name="TEntity"/></param>
        Task<ActionResult<TEntity>> GetAsync([FromRoute] string id);

        /// <summary>
        /// Return a list of <typeparamref name="TEntity"/>
        /// </summary>
        /// <remarks>
        /// In the implementation <see cref="EntityControllerBase{TEntity}"/>, the list of <typeparamref name="TEntity"/> is filtered by request parameters.
        /// </remarks>
        Task<ActionResult<IList<TEntity>>> GetAsync();

        /// <summary>
        /// Create a new <typeparamref name="TEntity"/> with the given data.
        /// </summary>
        /// <param name="model">An object that contains the data for the new <typeparamref name="TEntity"/></param>
        Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model);

        /// <summary>
        /// Update an existing <typeparamref name="TEntity"/> with the given data.
        /// </summary>
        /// <param name="id">The ID of the targeted <typeparamref name="TEntity"/></param>
        /// <param name="data">An object that contains the properties to update.</param>
        Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] object data);
    }

    /// <summary>
    /// Handles GET, POST, PATCH, and DELETE requests for <typeparamref name="TEntity"/>s
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle requests for</typeparam>
    /// <typeparam name="TEntityService">The service implementation for the <typeparamref name="TEntity"/></typeparam>
    public interface IEntityController<TEntity, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        
    }
}
