using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartModel
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        [JsonIgnore]
        public CustomerModel Customer { get; set; }
        [JsonIgnore]
        public List<CartItemModel> Items { get; set; }
    }
}
