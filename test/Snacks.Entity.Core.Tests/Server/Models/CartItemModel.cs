using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartItemModel
    {
        public int Id { get; set; }

        [ForeignKey("Cart")]
        public int CartId { get; set; }
        [JsonIgnore]
        public CartModel Cart { get; set; }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        [JsonIgnore]
        public ItemModel Item { get; set; }
    }
}
