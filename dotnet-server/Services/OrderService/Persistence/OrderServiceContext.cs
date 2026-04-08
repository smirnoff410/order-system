using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Persistence.Configuration;
namespace OrderService.Persistence
{
    public class OrderServiceContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public OrderServiceContext(DbContextOptions<OrderServiceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        }
    }
}
