using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Controllers;
using System;
using TestApplication.Models;

namespace TestApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : BaseEntityController<CustomerModel>
    {
        public CustomersController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
    }
}
