using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Helpers;

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

        protected IDbContextFactory<TDbContext> DbContextFactory { get; }
        protected ILogger Logger { get; }

        public EntityController(
            IDbContextFactory<TDbContext> dbContextFactory,
            ILogger<EntityController<TEntity, TKey, TDbContext>> logger)
        {
            DbContextFactory = dbContextFactory;
            Logger = logger;
        }

        /// <inheritdoc/>
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            using var dbContext = await DbContextFactory.CreateDbContextAsync();
            TEntity model = await dbContext.FindAsync<TEntity>(id);

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
            using var dbContext = await DbContextFactory.CreateDbContextAsync();
            var entities = dbContext.Set<TEntity>();
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
            using var dbContext = await DbContextFactory.CreateDbContextAsync();
            TEntity model = await dbContext.FindAsync<TEntity>(id);

            dbContext.Remove(model);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <inheritdoc/>
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            using var dbContext = await DbContextFactory.CreateDbContextAsync();
            EntityEntry<TEntity> entry = dbContext.Add(model);
            await dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        /// <inheritdoc/>
        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data)
        {
            using var dbContext = await DbContextFactory.CreateDbContextAsync();
            TEntity existingModel = await dbContext.FindAsync<TEntity>(id);

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

            dbContext.Update(existingModel);

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/{endpoint}")]
        public virtual async Task<IActionResult> GetRelatedAsync([FromRoute] TKey id, [FromRoute] string endpoint)
        {
            var result = await GetAsync(id);

            if (result.Value == null)
            {
                return result.Result ?? BadRequest();
            }

            var entity = result.Value;

            var relatedProperty = typeof(TEntity).GetProperties()
                .FirstOrDefault(x => x.Name.Equals(endpoint, StringComparison.InvariantCultureIgnoreCase));

            if (relatedProperty == default || typeof(ICollection<>).IsAssignableFrom(relatedProperty.PropertyType)) 
            {
                return NotFound();
            }

            var relatedEntityType = relatedProperty.PropertyType.GenericTypeArguments.First();

            using var dbContext = await DbContextFactory.CreateDbContextAsync();

            if (dbContext.Model.FindEntityType(relatedEntityType) == null)
            {
                return NotFound();
            }

            MethodInfo getRelated = this.GetType()
                .GetMethod(nameof(GetRelatedEntitiesAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(relatedEntityType);

            ParameterExpression entityParameterExp = Expression.Parameter(typeof(TEntity), "x");
            MemberExpression relatedPropertyExp = Expression.Property(entityParameterExp, relatedProperty);
            LambdaExpression relatedEntityExp = Expression.Lambda(relatedPropertyExp, entityParameterExp);
            
            dynamic relatedActionTask = getRelated.Invoke(this, new object [] { entity, Request.Query, relatedEntityExp });

            return Ok(await relatedActionTask);
        }

        protected async Task<IEnumerable<TRelatedEntity>> GetRelatedEntitiesAsync<TRelatedEntity>(TEntity entity, IQueryCollection query, LambdaExpression relatedExp)
            where TRelatedEntity : class
        {
            var relatedEntityType = typeof(TRelatedEntity);
            var queryableParameters = QueryableParameters.Build(query);
            var completeExpression = queryableParameters.ApplyLinqExpressions<TRelatedEntity, IEnumerable<TRelatedEntity>>(relatedExp);

            using var dbContext = await DbContextFactory.CreateDbContextAsync();

            if (entity != null)
            {
                var loadedEntities = await dbContext.Set<TEntity>()
                    .Where(x => x == entity)
                    .Include((Expression<Func<TEntity, IEnumerable<TRelatedEntity>>>)completeExpression)
                    .ToListAsync();

                var relatedEntities = loadedEntities.SelectMany(relatedExp.Compile() as Func<TEntity, IEnumerable<TRelatedEntity>>).ToList();

                return relatedEntities;
            }

            return default;
        }
    }
}
