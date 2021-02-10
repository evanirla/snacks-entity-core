using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Snacks.Entity.Authorization.Attributes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Snacks.Entity.Authorization
{
    public abstract class EntityAuthorizationHandlerBase<TEntity> : AuthorizationHandler<OperationAuthorizationRequirement, TEntity>
        where TEntity : class
    {
        public EntityAuthorizationHandlerBase()
        {

        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TEntity resource)
        {
            if (requirement.Name == Operations.Read.Name && typeof(TEntity).IsDefined(typeof(RestrictReadAttribute)))
            {
                RestrictReadAttribute restrictReadAttribute = typeof(TEntity).GetCustomAttribute<RestrictReadAttribute>();

                if (!restrictReadAttribute.Roles.Any(x => context.User.IsInRole(x)))
                {
                    context.Fail();
                }
            }
            else if (requirement.Name == Operations.Create.Name && typeof(TEntity).IsDefined(typeof(RestrictCreateAttribute)))
            {
                RestrictCreateAttribute restrictCreateAttribute = typeof(TEntity).GetCustomAttribute<RestrictCreateAttribute>();

                if (!restrictCreateAttribute.Roles.Any(x => context.User.IsInRole(x)))
                {
                    context.Fail();
                }
            }
            else if (requirement.Name == Operations.Update.Name && typeof(TEntity).IsDefined(typeof(RestrictUpdateAttribute)))
            {
                RestrictUpdateAttribute restrictUpdateAttribute = typeof(TEntity).GetCustomAttribute<RestrictUpdateAttribute>();

                if (!restrictUpdateAttribute.Roles.Any(x => context.User.IsInRole(x)))
                {
                    context.Fail();
                }
            }
            else if (requirement.Name == Operations.Delete.Name && typeof(TEntity).IsDefined(typeof(RestrictDeleteAttribute)))
            {
                RestrictDeleteAttribute restrictDeleteAttribute = typeof(TEntity).GetCustomAttribute<RestrictDeleteAttribute>();

                if (!restrictDeleteAttribute.Roles.Any(x => context.User.IsInRole(x)))
                {
                    context.Fail();
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
