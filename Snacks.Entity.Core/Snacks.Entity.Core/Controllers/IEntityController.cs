using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public interface IEntityController<TModel>
        where TModel : IEntityModel
    {
        Task<IActionResult> DeleteAsync(object key);
        Task<IActionResult> GetAsync(object key);
        Task<IActionResult> GetAsync();
        Task<IActionResult> PostAsync([FromBody] TModel model);
        Task<IActionResult> PostAsync([FromBody] List<TModel> models);
        Task<IActionResult> PutAsync(object key, [FromBody] TModel model);
    }

    public interface IEntityController<TModel, TKey> : IEntityController<TModel>
        where TModel : IEntityModel<TKey>
    {
        Task<IActionResult> DeleteAsync(TKey key);
        Task<IActionResult> GetAsync(TKey key);
        Task<IActionResult> PutAsync(TKey key, [FromBody] TModel model);
    }
}
