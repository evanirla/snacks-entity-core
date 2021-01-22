using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IEntityController<TEntity, TKey>
        where TEntity : class
    {
        Task<IActionResult> DeleteAsync([FromRoute] TKey id);
        Task<ActionResult<TEntity>> GetAsync([FromRoute] TKey id);
        Task<ActionResult<IList<TEntity>>> GetAsync();
        Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model);
        Task<IActionResult> PatchAsync([FromRoute] TKey id, [FromBody] object data);
    }

    public interface IEntityController<TEntity, TKey, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        
    }
}
