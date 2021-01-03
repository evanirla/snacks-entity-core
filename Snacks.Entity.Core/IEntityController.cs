using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IEntityController<TModel, TKey>
        where TModel : IEntityModel<TKey>
    {
        Task<IActionResult> DeleteAsync(TKey key);
        Task<ActionResult<TModel>> GetAsync(TKey key);
        Task<ActionResult<IList<TModel>>> GetAsync();
        Task<ActionResult<TModel>> PostAsync([FromBody] TModel model);
        Task<IActionResult> PatchAsync(TKey key, [FromBody] object data);
    }
}
