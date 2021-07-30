using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class CustomerService : EntityServiceBase<CustomerModel, SnacksDbContext>
    {
        public CustomerService(
            IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
            
        }
    }
}
