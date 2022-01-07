using Microsoft.AspNetCore.Mvc;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
