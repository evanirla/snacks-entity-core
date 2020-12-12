using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Entity;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public class BaseEntityController<TModel, TKey, TDbService, TDbConnection> : ControllerBase, IEntityController<TModel, TKey>
        where TModel : IEntityModel<TKey>
        where TDbService : IDbService<TDbConnection>
        where TDbConnection : IDbConnection
    {
        protected readonly IEntityService<TModel, TKey, TDbService, TDbConnection> _entityService;

        public BaseEntityController(IEntityService<TModel, TKey, TDbService, TDbConnection> entityService)
        {
            _entityService = entityService;
        }

        [HttpDelete("{key}")]
        public virtual async Task<IActionResult> DeleteAsync(TKey key)
        {
            TModel model = await _entityService.GetOneAsync(key);

            await _entityService.DeleteOneAsync(model);

            return Ok();
        }

        [HttpGet("{key}")]
        public virtual async Task<IActionResult> GetAsync(TKey key)
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
        public virtual async Task<IActionResult> PutAsync(TKey key, [FromBody] TModel model)
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
