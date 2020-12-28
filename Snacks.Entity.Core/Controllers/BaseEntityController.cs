using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public abstract class BaseEntityController<TModel, TKey> : ControllerBase, IEntityController<TModel, TKey>
        where TModel : IEntityModel<TKey>
    {
        protected readonly IServiceProvider _serviceProvider;

        private IEntityService<TModel> _entityService;
        protected IEntityService<TModel> EntityService
        {
            get
            {
                if (_entityService != null)
                {
                    return _entityService;
                }
                
                _entityService = _serviceProvider.GetRequiredService<IEntityService<TModel>>();

                return _entityService;
            }
        }

        public BaseEntityController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpDelete("{key}")]
        public virtual async Task<IActionResult> DeleteAsync(TKey key)
        {
            TModel model = await EntityService.GetOneAsync(key);

            await EntityService.DeleteOneAsync(model);

            return Ok();
        }

        [HttpGet("{key}")]
        public virtual async Task<ActionResult<TModel>> GetAsync(TKey key)
        {
            TModel model = await EntityService.GetOneAsync(key);

            if (model == null)
            {
                return NotFound();
            }

            return model;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IList<TModel>>> GetAsync()
        {
            return (await EntityService.GetManyAsync(Request.Query)).ToList();
        }

        [HttpPost]
        public virtual async Task<ActionResult<TModel>> PostAsync([FromBody] TModel model)
        {
            return await EntityService.CreateOneAsync(model);
        }

        [HttpPatch("{key}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TKey key, [FromBody] object data)
        {
            TModel existingModel = await EntityService.GetOneAsync(key);

            if (existingModel == null)
            {
                return NotFound();
            }

            await EntityService.UpdateOneAsync(existingModel, data);

            return Ok();
        }
    }
}
