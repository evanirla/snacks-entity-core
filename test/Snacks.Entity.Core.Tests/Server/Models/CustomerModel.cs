using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Snacks.Entity.Core.Tests.Server.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [JsonIgnore]
        public IList<CartModel> Carts { get; set; }
    }
}
