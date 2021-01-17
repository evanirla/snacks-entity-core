using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snacks.Entity.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public abstract class BaseEntityController<TEntity> : ControllerBase, IEntityController<TEntity>
        where TEntity : class
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();

        protected IEntityService<TEntity> Service { get; private set; }

        public BaseEntityController(IEntityService<TEntity> entityService)
        {
            Service = entityService;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] object id)
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

                await Service.AccessDbSetAsync(async Entities =>
                {
                    entities = await Entities.ToListAsync();
                });

                return entities;
            }
            else
            {
                List<TEntity> entities = default;

                await Service.AccessDbSetAsync(async Entities =>
                {
                    entities = await Entities.ApplyQueryParameters(Request.Query).ToListAsync();
                });

                return entities;
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] object id)
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
        public virtual async Task<IActionResult> PatchAsync([FromRoute] object id, [FromBody] object data)
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

    public abstract class BaseEntityController<TEntity, TEntityService> : BaseEntityController<TEntity>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public BaseEntityController(TEntityService entityService) : base(entityService)
        {
            
        }
    }
}
