using Snacks.Entity.Core.Attributes;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class Class : BaseEntityModel<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassId { get; set; }

        [Unique]
        public string Name { get; set; }
    }
}
