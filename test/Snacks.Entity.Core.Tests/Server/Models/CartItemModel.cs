using System.Text.Json.Serialization;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartItemModel
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        [JsonIgnore]
        public CartModel Cart { get; set; }

        public int ItemId { get; set; }
        [JsonIgnore]
        public ItemModel Item { get; set; }
    }
}
