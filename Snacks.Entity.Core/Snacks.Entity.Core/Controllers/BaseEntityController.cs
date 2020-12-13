using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public class BaseEntityController<TModel> : ControllerBase, IEntityController<TModel>
        where TModel : IEntityModel
    {
        protected readonly IEntityService<TModel> _entityService;

        public BaseEntityController(IServiceProvider serviceProvider)
        {
            _entityService = (IEntityService<TModel>)serviceProvider.GetService(typeof(IEntityService<TModel>));
        }

        [HttpDelete("{key}")]
        public virtual async Task<IActionResult> DeleteAsync(dynamic key)
        {
            TModel model = await _entityService.GetOneAsync(key);

            await _entityService.DeleteOneAsync(model);

            return Ok();
        }

        [HttpGet("{key}")]
        public virtual async Task<IActionResult> GetAsync(dynamic key)
        {
            TModel model = await _entityService.GetOneAsync(key);

            if (model == null)
            {
                return NotFound();
            }

            return new JsonResult(model);
        }

        [HttpGet("")]
        public virtual async Task<IActionResult> GetAsync()
        {
            return new JsonResult(await _entityService.GetManyAsync(Request.Query));
        }

        [HttpPost("")]
        public virtual async Task<IActionResult> PostAsync([FromBody] TModel model)
        {
            return new JsonResult(await _entityService.CreateOneAsync(model));
        }

        [HttpPost("multiple")]
        public virtual async Task<IActionResult> PostAsync([FromBody] List<TModel> models)
        {
            return new JsonResult(await _entityService.CreateManyAsync(models));
        }

        [HttpPut("{key}")]
        public virtual async Task<IActionResult> PutAsync(dynamic key, [FromBody] TModel model)
        {
            TModel existingModel = await _entityService.GetOneAsync(key);

            if (existingModel == null)
            {
                return NotFound();
            }

            model.Key = key;

            await _entityService.UpdateOneAsync(model);

            return Ok();
        }
    }
}
