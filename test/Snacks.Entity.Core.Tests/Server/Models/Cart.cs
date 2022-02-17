using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public decimal Total { get; set; }

        public int CustomerId { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set;}

        public List<CartItem> Items { get; set; }
    }
}
