using Snacks.Entity.Core.Tests.Server.Database;
using Snacks.Entity.Core.Tests.Server.Models;
using System;

namespace Snacks.Entity.Core.Tests.Server.Services
{
    public class CustomerService : EntityServiceBase<CustomerModel, GlobalDbContext>
    {
        public CustomerService(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
    }
}
