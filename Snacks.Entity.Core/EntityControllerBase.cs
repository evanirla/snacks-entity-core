using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public EntityControllerBase(IEntityService<TEntity> entityService)
        {
            Service = entityService;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id)
        {
            TEntity model = await Service.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            return model;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            if (Request.Query.Count == 0)
            {
                List<TEntity> entities = default;

                await Service.AccessEntitiesAsync(async Entities =>
                {
                    entities = await Entities.ToListAsync();
                });

                return entities;
            }
            else
            {
                List<TEntity> entities = default;

                await Service.AccessEntitiesAsync(async Entities =>
                {
                    entities = await Entities.ApplyQueryParameters(Request.Query).ToListAsync();
                });

                return entities;
            }
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

            return Ok();
        }

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            return await Service.CreateAsync(model).ConfigureAwait(false);
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

            await Service.UpdateAsync(existingModel);

            return Ok();
        }
    }

    /// <typeparam name="TEntityService"></typeparam>
    public abstract class EntityControllerBase<TEntity, TKey, TEntityService> : EntityControllerBase<TEntity, TKey>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public EntityControllerBase(TEntityService entityService) : base(entityService)
        {
            
        }
    }
}
