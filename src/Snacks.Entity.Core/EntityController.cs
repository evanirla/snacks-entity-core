﻿using Microsoft.AspNetCore.Mvc;
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

            if (DbContext.Model.FindEntityType(relatedEntityType) == null)
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

            if (entity != null)
            {
                var loadedEntities = await DbContext.Set<TEntity>()
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
