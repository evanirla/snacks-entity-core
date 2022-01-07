using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class ItemService : EntityServiceBase<ItemModel, GlobalDbContext>
    {
        public ItemService(
            IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {

        }
    }
}
