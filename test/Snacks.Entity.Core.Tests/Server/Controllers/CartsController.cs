using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using System;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : EntityControllerBase<CartModel>
    {
        public CartsController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
    }
}
