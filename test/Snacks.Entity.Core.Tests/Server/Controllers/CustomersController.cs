using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snacks.Entity.Core.Extensions;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<IList<CartModel>>> GetCartsAsync([FromRoute] string id) => await GetRelatedAsync(Request, customer => customer.Carts);
    }
}
