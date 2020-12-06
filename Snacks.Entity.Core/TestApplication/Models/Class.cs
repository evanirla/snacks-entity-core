using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class Class : BaseEntityModel<int>
    {
        public string Name { get; set; }
    }
}
