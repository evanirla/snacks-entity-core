using System.ComponentModel.DataAnnotations.Schema;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CartItemModel
    {
        public int Id { get; set; }

        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public CartModel Cart { get; set; }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public ItemModel Item { get; set; }
    }
}
