using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class OtherModel : BaseEntityModel<int>
    {
        [Key]
        public int Id { get; set; }
    }
}
