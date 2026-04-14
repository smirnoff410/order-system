using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using ProductService.Persistence.Configuration;

namespace ProductService.Persistence
{
    public class ProductServiceContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public ProductServiceContext(DbContextOptions<ProductServiceContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }
    }
}
