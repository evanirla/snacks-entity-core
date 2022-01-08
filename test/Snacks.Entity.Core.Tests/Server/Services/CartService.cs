using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;
using System;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class CartService : EntityServiceBase<CartModel, GlobalDbContext>
    {
        public CartService(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
    }
}
