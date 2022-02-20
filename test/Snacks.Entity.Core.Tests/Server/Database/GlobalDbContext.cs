using Microsoft.EntityFrameworkCore;
using Snacks.Entity.Core.Tests.Server.Models;

namespace Snacks.Entity.Core.Tests.Server.Database
{
    public class GlobalDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ItemModel> Items { get; set; }

        public GlobalDbContext(DbContextOptions<GlobalDbContext> options)
            : base(options)
        {
            
        }
    }
}
