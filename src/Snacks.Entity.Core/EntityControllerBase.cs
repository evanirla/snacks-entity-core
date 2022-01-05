using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snacks.Entity.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <inheritdoc/>
    public abstract class EntityControllerBase<TEntity> : ControllerBase, IEntityController<TEntity>
        where TEntity : class
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();
        
        private IServiceProvider _serviceProvider; 

        protected ILogger<EntityControllerBase<TEntity>> Logger { get; private set; }

        /// <summary>
        /// Provides CRUD operations for <typeparamref name="TEntity"/>
        /// </summary>
        protected IEntityService<TEntity> Service { get; private set; }

        /// <summary>
        /// Provides a simple interface for caching action results
        /// </summary>
        protected DistributedCacheHelper<EntityControllerBase<TEntity>> Cache { get; private set; }

        public EntityControllerBase(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Cache = new DistributedCacheHelper<EntityControllerBase<TEntity>>(
                serviceProvider.GetService<IDistributedCache>()
            );
            Logger = serviceProvider.GetRequiredService<ILogger<EntityControllerBase<TEntity>>>();
            Service = serviceProvider.GetRequiredService<IEntityService<TEntity>>();
        }

        /// <inheritdoc/>
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] string id)
        {
            TEntity model = await Cache.GetFromRequestAsync<TEntity>(Request);

            if (model != null)
            {
                return model;
            }

            model = await Service.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            await Cache.AddRequestAsync(Request, model);

            return model;
        }

        /// <inheritdoc/>
        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            List<TEntity> models = await Cache.GetFromRequestAsync<List<TEntity>>(Request);

            if (models != null)
            {
                return models;
            }

            await Service.AccessEntitiesAsync(async Entities =>
            {
                if (Request.Query.Count == 0)
                {
                    models = await Entities.ToListAsync().ConfigureAwait(false);
                }
                else
                {
                    models = await Entities.ApplyQueryParameters(Request.Query).ToListAsync().ConfigureAwait(false);
                }
            });

            await Cache.AddRequestAsync(Request, models);

            return models;
        }

        /// <inheritdoc/>
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            TEntity model = await Service.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            await Service.DeleteAsync(model).ConfigureAwait(false);

            await Cache.PurgeAsync();

            return Ok();
        }

        /// <inheritdoc/>
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            TEntity newModel = await Service.CreateAsync(model).ConfigureAwait(false);

            await Cache.PurgeAsync();

            return newModel;
        }

        /// <inheritdoc/>
        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] object data)
        {
            TEntity existingModel = await Service.FindAsync(id).ConfigureAwait(false);

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

            await Service.UpdateAsync(existingModel).ConfigureAwait(false);

            await Cache.PurgeAsync();

            return Ok();
        }

        protected async Task<ActionResult<IEnumerable<TRelatedEntity>>> GetRelatedAsync<TRelatedEntity>(string id, IQueryCollection query, Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> relatedExp)
        {
            var result = await GetAsync(id);
            var entity = result.Value;

            var relatedEntityType = ((relatedExp.Body as MemberExpression).Member as PropertyInfo).PropertyType.GenericTypeArguments[0];

            MethodInfo filterExpFromQuery = typeof(IEnumerableExtensions).GetMethod(nameof(IEnumerableExtensions.FilterExpressionsFromQueryParameters));
            filterExpFromQuery = filterExpFromQuery.MakeGenericMethod(relatedEntityType);
            var filterExpressions = filterExpFromQuery.Invoke(null, new [] { query }) as IEnumerable<LambdaExpression>;

            var whereMethod = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == nameof(Enumerable.Where));
            whereMethod = whereMethod.MakeGenericMethod(relatedEntityType);

            LambdaExpression includeExpression = relatedExp;

            foreach (LambdaExpression expression in filterExpressions)
            {
                includeExpression = Expression.Lambda(
                    Expression.Call(null, whereMethod, relatedExp.Body, expression),
                    relatedExp.Parameters[0]
                );
            }

            if (entity != null)
            {
                return await Service.AccessEntitiesAsync(async dbSet =>
                {
                    var loadedEntities = await dbSet
                        .Where(x => x == entity)
                        .Include((Expression<Func<TEntity, IEnumerable<TRelatedEntity>>>)includeExpression)
                        .ToListAsync();
                    return loadedEntities.SelectMany(relatedExp.Compile()).ToList();
                });
            }

            return default;
        }
    }

    /// <inheritdoc/>
    public abstract class EntityControllerBase<TEntity, TEntityService> : EntityControllerBase<TEntity>, IEntityController<TEntity, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public EntityControllerBase(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
    }
}
