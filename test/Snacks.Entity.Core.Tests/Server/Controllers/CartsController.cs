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
    public class CartsController : EntityControllerBase<CartModel, CartService>
    {
        public CartsController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }

        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<CartItemModel>>> GetItemsAsync([FromRoute] string id) =>
            await GetRelatedAsync(id, Request.Query, cart => cart.Items, item => item.Item);
    }
}
