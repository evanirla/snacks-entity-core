using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public interface IEntityController<TEntity>
        where TEntity : class
    {
        Task<IActionResult> DeleteAsync([FromRoute] object id);
        Task<ActionResult<TEntity>> GetAsync([FromRoute] object id);
        Task<ActionResult<IList<TEntity>>> GetAsync();
        Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model);
        Task<IActionResult> PatchAsync([FromRoute] object id, [FromBody] object data);
    }

    public interface IEntityController<TEntity, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        
    }
}
