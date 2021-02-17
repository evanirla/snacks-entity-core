using System.Collections.Generic;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<CartModel> Carts { get; set; }
    }
}
