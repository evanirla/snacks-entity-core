using System.ComponentModel.DataAnnotations.Schema;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartModel
    {
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public CustomerModel Customer { get; set; }
    }
}
