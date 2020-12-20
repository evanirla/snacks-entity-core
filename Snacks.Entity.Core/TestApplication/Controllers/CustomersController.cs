using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestApplication.Models;

namespace TestApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : BaseEntityController<CustomerModel, int>
    {
        public CustomersController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }

        [HttpGet]
        public override async Task<ActionResult<IList<CustomerModel>>> GetAsync()
        {
            await EntityService.CreateOneAsync(new CustomerModel
            {

            });

            return await base.GetAsync();
        }
    }
}
