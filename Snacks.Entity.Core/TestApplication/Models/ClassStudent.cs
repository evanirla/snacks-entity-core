using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    public class ClassStudent : BaseEntityModel<int>
    {
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }
    }
}
