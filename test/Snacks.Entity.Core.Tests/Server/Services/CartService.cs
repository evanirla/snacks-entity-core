using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class CartService : EntityServiceBase<CartModel, GlobalDbContext>
    {
        public CartService(
            IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {

        }
    }
}
