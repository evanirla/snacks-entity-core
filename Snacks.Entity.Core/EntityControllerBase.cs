using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Snacks.Entity.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// Handles GET, POST, PATCH, and DELETE web requests for a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for the controller</typeparam>
    /// <typeparam name="TKey">A primitive type that matches the key type of the entity.</typeparam>
    public abstract class EntityControllerBase<TEntity, TKey> : ControllerBase, IEntityController<TEntity, TKey>
        where TEntity : class
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();

        protected IEntityService<TEntity> Service { get; private set; }
        protected GlobalCacheService<TEntity> GlobalCache { get; private set; }
        protected UserCacheService<TEntity> UserCache { get; private set; }

        public EntityControllerBase(
            IEntityService<TEntity> entityService,
            IDistributedCache distributedCache = null)
        {
            if (distributedCache != null)
            {
                // is there any performance degradation here since controllers are scoped?
                GlobalCache = new GlobalCacheService<TEntity>(distributedCache);
            }
            
            Service = entityService;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            if (GlobalCache != null)
            {
                TEntity cachedModel = await GlobalCache.FindAsync(Request).ConfigureAwait(false);
                if (cachedModel != null)
                {
                    return cachedModel;
                }
            }

            TEntity model = await Service.FindAsync(id).ConfigureAwait(false);
            if (model == null)
            {
                return NotFound();
            }

            if (GlobalCache != null)
            {
                await GlobalCache.AddAsync(Request, model).ConfigureAwait(false);
            }

            return model;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            if (GlobalCache != null)
            {
                IList<TEntity> cachedModels = await GlobalCache.GetAsync(Request).ConfigureAwait(false);

                if (cachedModels != null)
                {
                    return cachedModels.ToList();
                }
            }

            List<TEntity> models = default;

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

            if (GlobalCache != null)
            {
                await GlobalCache.AddAsync(Request, models).ConfigureAwait(false);
            }

            return models;
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] TKey id)
        {
            TEntity model = await Service.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            await Service.DeleteAsync(model).ConfigureAwait(false);

            if (GlobalCache != null)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return Ok();
        }

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            TEntity newModel = await Service.CreateAsync(model).ConfigureAwait(false);

            if (GlobalCache != null)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return newModel;
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data)
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

            if (GlobalCache != null)
            {
                await GlobalCache.PurgeAsync().ConfigureAwait(false);
            }

            return Ok();
        }
    }

    public abstract class EntityControllerBase<TEntity, TKey, TEntityService> : EntityControllerBase<TEntity, TKey>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public EntityControllerBase(
            TEntityService entityService,
            IDistributedCache distributedCache = null) : base(entityService, distributedCache)
        {
            
        }
    }
}
