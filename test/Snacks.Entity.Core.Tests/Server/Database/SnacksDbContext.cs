using Microsoft.EntityFrameworkCore;
using Snacks.Entity.Core.Tests.Server.Models;

namespace Snacks.Entity.Core.Tests.Server.Database
{
    public class SnacksDbContext : DbContext
    {
        public DbSet<CustomerModel> Customers { get; set; }
        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CartItemModel> CartItems { get; set; }
        public DbSet<ItemModel> Items { get; set; }

        public SnacksDbContext(DbContextOptions<SnacksDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
