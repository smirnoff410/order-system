using Microsoft.EntityFrameworkCore;
using StockService.Models;
using StockService.Persistence.Configuration;

namespace StockService.Persistence
{
    public class StockServiceContext : DbContext
    {
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public StockServiceContext(DbContextOptions<StockServiceContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ReservationsConfiguration());
            modelBuilder.ApplyConfiguration(new StockItemConfiguration());
        }
    }
}
