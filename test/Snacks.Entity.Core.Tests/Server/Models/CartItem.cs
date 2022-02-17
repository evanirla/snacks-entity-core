using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int CartId { get; set; }
        [JsonIgnore]
        public Cart Cart { get; set; }

        public int ItemId { get; set; }
        public ItemModel Item { get; set; }
    }
}
