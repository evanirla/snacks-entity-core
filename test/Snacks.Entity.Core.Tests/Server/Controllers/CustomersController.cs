using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : EntityControllerBase<CustomerModel, CustomerService>
    {
        public CustomersController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }

        [HttpGet("{id}/carts")]
        public async Task<ActionResult<IEnumerable<CartModel>>> GetCartsAsync([FromRoute] string id) =>
            await GetRelatedAsync(id, Request.Query, customer => customer.Carts);
    }
}
