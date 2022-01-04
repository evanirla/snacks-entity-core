using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartModel
    {
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        [JsonIgnore]
        public CustomerModel Customer { get; set; }
        [JsonIgnore]
        public ICollection<CartItemModel> Items { get; set; }

        public decimal Total { get; set; }
    }
}
