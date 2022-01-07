using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Authorization
{
    public abstract class SecuredEntityControllerBase<TEntity> : EntityControllerBase<TEntity>
        where TEntity : class
    {
        protected readonly IAuthorizationService AuthorizationService;

        public SecuredEntityControllerBase(
            IServiceProvider serviceProvider
        ) : base(serviceProvider)
        {
            AuthorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        }

        public override async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            ActionResult<IList<TEntity>> result = await base.GetAsync();

            if (!(result.Result is OkResult))
            {
                return result;
            }

            foreach (TEntity entity in result.Value)
            {
                AuthorizationResult authorizationResult = 
                    await AuthorizationService.AuthorizeAsync(User, entity, Operations.Read);

                if (!authorizationResult.Succeeded)
                {
                    result.Value.Remove(entity);
                }
            }

            return result;
        }

        public override async Task<ActionResult<TEntity>> GetAsync([FromRoute] string id)
        {
            ActionResult<TEntity> result = await base.GetAsync(id);

            if (!(result.Result is OkResult))
            {
                return result;
            }

            AuthorizationResult authorizationResult =
                await AuthorizationService.AuthorizeAsync(User, result.Value, Operations.Read);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return result;
        }

        public override async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            ActionResult<TEntity> result = await base.PostAsync(model);

            if (!(result.Result is OkResult))
            {
                return result;
            }

            AuthorizationResult authorizationResult =
                await AuthorizationService.AuthorizeAsync(User, result.Value, Operations.Create);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return result;
        }

        public override async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] object data)
        {
            TEntity entity = await Service.FindAsync(id);

            AuthorizationResult authorizationResult =
                await AuthorizationService.AuthorizeAsync(User, entity, Operations.Update);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return await base.PatchAsync(id, data);
        }

        public override async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            TEntity entity = await Service.FindAsync(id);

            AuthorizationResult authorizationResult =
                await AuthorizationService.AuthorizeAsync(User, entity, Operations.Delete);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return await base.DeleteAsync(id);
        }
    }

    public abstract class SecuredEntityControllerBase<TEntity, TEntityService> : SecuredEntityControllerBase<TEntity>, IEntityController<TEntity, TEntityService>
        where TEntity : class
        where TEntityService : IEntityService<TEntity>
    {
        new protected TEntityService Service => (TEntityService)base.Service;

        public SecuredEntityControllerBase(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
    }
}
