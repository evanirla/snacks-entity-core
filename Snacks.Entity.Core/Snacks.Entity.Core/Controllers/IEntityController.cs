using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Controllers
{
    public interface IEntityController<TModel, TKey>
        where TModel : IEntityModel<TKey>
    {
        Task<IActionResult> DeleteAsync(TKey key);
        Task<IActionResult> GetAsync(TKey key);
        Task<IActionResult> GetAsync();
        Task<IActionResult> PostAsync([FromBody] TModel model);
        Task<IActionResult> PostAsync([FromBody] List<TModel> models);
        Task<IActionResult> PutAsync(TKey key, [FromBody] TModel model);
    }
}
