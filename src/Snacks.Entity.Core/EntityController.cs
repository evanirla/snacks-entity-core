using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <inheritdoc/>
    [ApiController]
    [Route("api/[controller]")]
    public class EntityController<TEntity, TKey, TDbContext> : ControllerBase, IEntityController<TEntity, TKey, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();

        protected TDbContext DbContext { get; private set; }

        protected ILogger Logger { get; private set; }

        public EntityController(
            TDbContext dbContext,
            ILogger<EntityController<TEntity, TKey, TDbContext>> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        /// <inheritdoc/>
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            TEntity model = await DbContext.FindAsync<TEntity>(id);

            if (model == null)
            {
                return NotFound();
            }

            return model;
        }

        /// <inheritdoc/>
        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            var entities = DbContext.Set<TEntity>();
            if (Request.Query.Count == 0)
            {
                return await entities.ToListAsync();
            }
            else
            {
                return await entities.ApplyQueryParameters(Request.Query).ToListAsync();
            }
        }

        /// <inheritdoc/>
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] TKey id)
        {
            TEntity model = await DbContext.FindAsync<TEntity>(id);

            DbContext.Remove(model);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <inheritdoc/>
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            EntityEntry<TEntity> entry = DbContext.Add(model);
            await DbContext.SaveChangesAsync();
            return entry.Entity;
        }

        /// <inheritdoc/>
        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data)
        {
            TEntity existingModel = await DbContext.FindAsync<TEntity>(id);

            if (existingModel == null)
            {
                return NotFound();
            }

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                PropertyInfo entityProperty = _entityProperties.FirstOrDefault(
                    x => x.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                if (entityProperty != null)
                {
                    entityProperty.SetValue(existingModel, property.GetValue(data));
                }
            }

            DbContext.Update(existingModel);

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Returns filtered related entity data wrapped in an <see cref="ActionResult" />
        /// </summary>
        /// <example>
        /// <code>
        /// [HttpGet("{id}/carts")]
        /// public async Task<ActionResult<IEnumerable<CartModel>>> GetCarts([FromRoute]string id) =>
        ///     await GetRelatedAsync<CartModel>(id, Request.Query, customer => customer.Carts);
        /// </code>
        /// </example>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="relatedExp"></param>
        /// <typeparam name="TRelatedEntity"></typeparam>
        /// <returns></returns>
        protected async Task<ActionResult<IEnumerable<TRelatedEntity>>> GetRelatedAsync<TRelatedEntity>(TKey id, IQueryCollection query, Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> relatedExp)
            where TRelatedEntity : class
        {
            var result = await GetAsync(id);

            if (result.Value == null)
            {
                return result.Result ?? BadRequest();
            }

            var entity = result.Value;

            var relatedEntityType = typeof(TRelatedEntity);
            var queryableParameters = QueryableParameters.Build(query);
            var completeExpression = queryableParameters.ApplyLinqExpressions<TRelatedEntity, IEnumerable<TRelatedEntity>>(relatedExp);

            if (entity != null)
            {
                var loadedEntities = await DbContext.Set<TEntity>()
                    .Where(x => x == entity)
                    .Include((Expression<Func<TEntity, IEnumerable<TRelatedEntity>>>)completeExpression)
                    .ToListAsync();
                
                return loadedEntities.SelectMany(relatedExp.Compile()).ToList();
            }

            return default;
        }

        /// <summary>
        /// Returns filtered related entity data wrapped in an <see cref="ActionResult" /> and includes <see cref="TRelatedEntity2" />
        /// </summary>
        /// <example>
        /// <code>
        /// [HttpGet("{id}/items")]
        /// public async Task<ActionResult<IEnumerable<CartItemModel>>> GetItems([FromRoute]string id) =>
        ///     await GetRelatedAsync<CartItemModel>(id, Request.Query, cart => customer.Items, item => item.Item);
        /// </code>
        /// </example>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="relatedExp"></param>
        /// <param name="relatedExp2"></param>
        /// <typeparam name="TRelatedEntity"></typeparam>
        /// <typeparam name="TRelatedEntity2"></typeparam>
        /// <returns></returns>
        protected async Task<ActionResult<IEnumerable<TRelatedEntity>>> GetRelatedAsync<TRelatedEntity, TRelatedEntity2>(
            TKey id, 
            IQueryCollection query, 
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> relatedExp,
            Expression<Func<TRelatedEntity, TRelatedEntity2>> relatedExp2
        )
            where TRelatedEntity : class
        {
            var result = await GetAsync(id);

            if (result.Value == null)
            {
                return result.Result ?? BadRequest();
            }

            var entity = result.Value;

            var queryableParameters = QueryableParameters.Build(query);
            var completeExpression = queryableParameters.ApplyLinqExpressions<TRelatedEntity, IEnumerable<TRelatedEntity>>(relatedExp);

            if (entity != null)
            {
                var loadedEntities = await DbContext.Set<TEntity>()
                    .Where(x => x == entity)
                    .Include((Expression<Func<TEntity, IEnumerable<TRelatedEntity>>>)completeExpression)
                    .ThenInclude(relatedExp2)
                    .ToListAsync();
                return loadedEntities.SelectMany(relatedExp.Compile()).ToList();
            }

            return default;
        }
    }
}
