using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;
using System;
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
    }
}
