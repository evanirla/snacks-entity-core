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
    /// <inheritdoc/>
    public abstract class EntityControllerBase<TEntity, TKey> : ControllerBase, IEntityController<TEntity, TKey>
        where TEntity : class
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();

        /// <summary>
        /// Provides CRUD operations for <typeparamref name="TEntity"/>
        /// </summary>
        protected IEntityService<TEntity> Service { get; private set; }

        public EntityControllerBase(
            IEntityService<TEntity> entityService)
        {
            Service = entityService;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
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

            return models;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            TEntity newModel = await Service.CreateAsync(model).ConfigureAwait(false);

            return newModel;
        }

        /// <inheritdoc/>
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

            return Ok();
        }
    }

    /// <inheritdoc/>
    public abstract class EntityControllerBase<TEntity, TKey, TEntityService> : EntityControllerBase<TEntity, TKey>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        new protected TEntityService Service => (TEntityService)base.Service;

        public EntityControllerBase(
            TEntityService entityService) : base(entityService)
        {
            
        }
    }
}
