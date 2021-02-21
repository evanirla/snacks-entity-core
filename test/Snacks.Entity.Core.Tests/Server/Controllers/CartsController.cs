using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : EntityControllerBase<CartModel, int, CartService>
    {
        public CartsController(
            IEntityService<CartModel> cartService) : base((CartService)cartService)
        {
            
        }
    }
}
