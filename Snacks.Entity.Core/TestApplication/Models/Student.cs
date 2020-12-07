using Snacks.Entity.Core;
using Snacks.Entity.Core.Attributes;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class Student : BaseEntityModel<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Unique]
        public string Name { get; set; }
    }
}
