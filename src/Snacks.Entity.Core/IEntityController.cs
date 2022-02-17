using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Handles GET, POST, PATCH, and DELETE requests for <typeparamref name="TEntity"/>s
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle requests for</typeparam>
    public interface IEntityController<TEntity, TKey, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        /// <summary>
        /// Deletes the <typeparamref name="TEntity"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the targeted <typeparamref name="TEntity"/></param>
        Task<IActionResult> DeleteAsync([FromRoute] TKey id);

        /// <summary>
        /// Return the <typeparamref name="TEntity"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the targeted <typeparamref name="TEntity"/></param>
        Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id);

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
        Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data);
    }
}
