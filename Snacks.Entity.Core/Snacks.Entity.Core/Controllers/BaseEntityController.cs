using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public class BaseEntityController<TModel> : ControllerBase, IEntityController<TModel>
        where TModel : IEntityModel
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
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] object key)
        {
            TModel model = await EntityService.GetOneAsync(key);

            await EntityService.DeleteOneAsync(model);

            return Ok();
        }

        [HttpGet("{key}")]
        public virtual async Task<IActionResult> GetAsync([FromRoute]object key)
        {
            TModel model = await EntityService.GetOneAsync(key);

            if (model == null)
            {
                return NotFound();
            }

            return new JsonResult(model);
        }

        [HttpGet("")]
        public virtual async Task<IActionResult> GetAsync()
        {
            return new JsonResult(await EntityService.GetManyAsync(Request.Query));
        }

        [HttpPost("")]
        public virtual async Task<IActionResult> PostAsync([FromBody] TModel model)
        {
            return new JsonResult(await EntityService.CreateOneAsync(model));
        }

        [HttpPost("multiple")]
        public virtual async Task<IActionResult> PostAsync([FromBody] List<TModel> models)
        {
            return new JsonResult(await EntityService.CreateManyAsync(models));
        }

        [HttpPut("{key}")]
        public virtual async Task<IActionResult> PutAsync([FromRoute]object key, [FromBody] TModel model)
        {
            TModel existingModel = await EntityService.GetOneAsync(key);

            if (existingModel == null)
            {
                return NotFound();
            }

            model.Key = key;

            await EntityService.UpdateOneAsync(model);

            return Ok();
        }
    }
}
