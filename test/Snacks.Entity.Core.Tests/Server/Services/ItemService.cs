using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;
using System;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class ItemService : EntityServiceBase<ItemModel, GlobalDbContext>
    {
        public ItemService(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
    }
}
