﻿using Microsoft.AspNetCore.Mvc;
using Snacks.Entity.Core.Tests.Server.Models;
using Snacks.Entity.Core.Tests.Server.Services;

namespace Snacks.Entity.Core.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : EntityControllerBase<ItemModel, int, ItemService>
    {
        public ItemsController(
            IEntityService<ItemModel> itemService) : base((ItemService)itemService)
        {
            
        }
    }
}