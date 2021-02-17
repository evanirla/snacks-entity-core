using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : EntityControllerBase<CustomerModel, int, CustomerService>
    {
        public CustomersController(
            IEntityService<CustomerModel> customerService) : base((CustomerService)customerService)
        {
            
        }
    }
}
