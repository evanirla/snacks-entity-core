using Snacks.Entity.Core.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestApplication.Models
{
    public class CustomerModel : BaseEntityModel<int>
    {
        [Key]
        [Column("id")]
        public int CustomerId { get; set; }
    }
}
