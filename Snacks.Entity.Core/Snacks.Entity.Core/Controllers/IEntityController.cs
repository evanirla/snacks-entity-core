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
        Task<ActionResult<TModel>> GetAsync(TKey key);
        Task<ActionResult<IList<TModel>>> GetAsync();
        Task<ActionResult<TModel>> PostAsync([FromBody] TModel model);
        Task<ActionResult<IList<TModel>>> PostAsync([FromBody] List<TModel> models);
        Task<IActionResult> PutAsync(TKey key, [FromBody] TModel model);
    }
}
